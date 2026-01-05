using Application.Features.Users.Commands;
using Application.Features.Users.Commands.CreateUser;
using Application.Features.Users.DTOs;
using Domain.SeedWork;
using Infrastructure.Identity.Models;
using MediatR;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ISender _sender;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IOpenIddictApplicationManager _applicationManager;
        private readonly IUnitOfWork _unitOfWork;

        public AuthController(ISender sender, UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork, SignInManager<ApplicationUser> signInManager, IOpenIddictApplicationManager applicationManager)
        {
            _sender = sender;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _signInManager = signInManager;
            _applicationManager = applicationManager;
        }

        /// <summary>
        /// Yeni bir kullanıcı kaydı oluşturur (Identity + Lokal Profil).
        /// </summary>
        [HttpPost("/register")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            // 1. Transaction'ı MANUEL olarak başlatıyoruz
            // Çünkü hem Infrastructure (UserManager) hem de Application (Handler)
            // aynı transaction'da çalışmalı.
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // 2. Identity (AspNetUsers) Varlığını Oluştur
                var identityUser = new ApplicationUser
                {
                    Email = request.Email,
                    UserName = request.UserName,
                    EmailConfirmed = true
                };

                var identityResult = await _userManager.CreateAsync(identityUser, request.Password);
                if (!identityResult.Succeeded)
                {
                    await _unitOfWork.RollbackTransactionAsync(); // Hata olursa geri al
                    return BadRequest(identityResult.Errors);
                }

                // 3. Application (Lokal Profil) Varlığını Oluştur
                var command = new CreateUserCommand
                {
                    IdentityId = identityUser.Id,
                    UserName = identityUser.UserName,
                    Email = identityUser.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName
                };

                // MediatR handler'ını çağır (Bu da SaveChangesAsync'i çağıracak)
                var userId = await _sender.Send(command);

                // 4. Transaction'ı Onayla
                await _unitOfWork.CommitTransactionAsync();

                // 5. Profili DTO olarak dön (veya token bas)
                // Şimdilik sadece OK dönelim, kullanıcı /connect/token'a gitsin
                return Ok(new { UserId = userId, IdentityId = identityUser.Id });
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        // OpenIddict Token Endpoint'i (/connect/token)
        // Bu metot, OpenIddict'in Password Flow'unu tetikler.
        /// </summary>
        #region connect/token
        [HttpPost("~/connect/token")]
        [Consumes("application/x-www-form-urlencoded")] // Swagger'a Form tipi olduğunu söyle
        [Produces("application/json")]
        public async Task<IActionResult> Exchange([FromForm] TokenRequest request)
        {
            // Gelen OIDC isteğini al
            var openIddictRequest = HttpContext.GetOpenIddictServerRequest();
            if (openIddictRequest == null) throw new InvalidOperationException("İstek okunamadı.");

            // 1. SENARYO: S2S (LogisticsAPI gibi servisler için)
            if (openIddictRequest.IsClientCredentialsGrantType())
            {
                // Client ID'yi al (ClientRegistrationWorker'da kaydettiğimiz)
                var applicationId = openIddictRequest.ClientId;

                // Client'ın varlığını doğrula (OpenIddict bunu zaten yapmış olabilir ama güvenli taraf)
                var application = await _applicationManager.FindByClientIdAsync(applicationId);
                if (application == null)
                {
                    return Forbid(
                        authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                        properties: new Microsoft.AspNetCore.Authentication.AuthenticationProperties(new Dictionary<string, string>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidClient,
                            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "İstemci bulunamadı."
                        }));
                }

                // Yeni bir kimlik (ClaimsPrincipal) oluştur
                // Bu bir "insan" değil, bir "servis" olduğu için User tablosuna bakmıyoruz.
                var identity = new ClaimsIdentity(
                    authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                    nameType: Claims.Name,
                    roleType: Claims.Role);

                // Kimliğe gerekli claim'leri ekle
                identity.AddClaim(Claims.Subject, applicationId); // ID olarak ClientID kullan
                identity.AddClaim(Claims.Name, await _applicationManager.GetDisplayNameAsync(application));

                // *** KRİTİK NOKTA ***
                // LogisticsAPI'nin [Authorize(Policy = "InternalApiAccess")] politikasını
                // geçebilmesi için 'client_id' claim'ini ekliyoruz.
                // OpenIddict bunu genelde otomatik yapar ama biz garantiye alalım.
                identity.AddClaim("client_id", applicationId);

                // Token'a eklenecek hakları (Scopes) belirle
                identity.SetScopes(openIddictRequest.GetScopes());
                identity.SetDestinations(GetDestinations);

                // Token'ı üret ve dön
                return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            // 2. SENARYO: KULLANICI (Mobil/Web - Password Grant)
            if (openIddictRequest.IsPasswordGrantType())
            {
                var user = await _userManager.FindByNameAsync(openIddictRequest.Username);
                if (user == null)
                {
                    return Forbid(
                        authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                        properties: new Microsoft.AspNetCore.Authentication.AuthenticationProperties(new Dictionary<string, string>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "Kullanıcı adı veya şifre hatalı."
                        }));
                }

                // Şifreyi kontrol et
                var result = await _signInManager.CheckPasswordSignInAsync(user, openIddictRequest.Password, lockoutOnFailure: true);
                if (!result.Succeeded)
                {
                    return Forbid(
                        authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                        properties: new Microsoft.AspNetCore.Authentication.AuthenticationProperties(new Dictionary<string, string>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "Kullanıcı adı veya şifre hatalı."
                        }));
                }

                // Email onayı kontrolü (Program.cs'te zorunlu kıldıysak)
                if (_userManager.Options.SignIn.RequireConfirmedEmail && !await _userManager.IsEmailConfirmedAsync(user))
                {
                    return Forbid(
                       authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                       properties: new Microsoft.AspNetCore.Authentication.AuthenticationProperties(new Dictionary<string, string>
                       {
                           [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                           [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "Lütfen önce email adresinizi onaylayın."
                       }));
                }

                // Kullanıcı için Principal oluştur
                var principal = await _signInManager.CreateUserPrincipalAsync(user);

                var identity = (ClaimsIdentity)principal.Identity;

                if (!principal.HasClaim(c => c.Type == Claims.Subject))
                {
                    identity.AddClaim(new Claim(
                        type: Claims.Subject,
                        value: await _userManager.GetUserIdAsync(user)) // Kullanıcı ID'sini basıyoruz
                    );
                }

                // Scope ve Destination ayarlarını yap
                principal.SetScopes(openIddictRequest.GetScopes());
                foreach (var claim in principal.Claims)
                {
                    claim.SetDestinations(GetDestinations(claim, principal));
                }

                return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            // 3. SENARYO: Refresh Token
            if (openIddictRequest.IsRefreshTokenGrantType())
            {
                // Refresh token ile gelen identity'yi al
                var info = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                var user = await _userManager.GetUserAsync(info.Principal);

                if (user == null)
                {
                    return Forbid(
                       authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                       properties: new Microsoft.AspNetCore.Authentication.AuthenticationProperties(new Dictionary<string, string>
                       {
                           [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                           [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "Refresh token artık geçerli değil."
                       }));
                }

                // Kullanıcının hala giriş yapabilir durumda olduğunu kontrol et (banlanmış mı vb.)
                if (!await _signInManager.CanSignInAsync(user))
                {
                    return Forbid(
                       authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                       properties: new Microsoft.AspNetCore.Authentication.AuthenticationProperties(new Dictionary<string, string>
                       {
                           [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                           [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "Kullanıcı artık giriş yapamaz."
                       }));
                }

                var principal = await _signInManager.CreateUserPrincipalAsync(user);
                foreach (var claim in principal.Claims)
                {
                    claim.SetDestinations(GetDestinations(claim, principal));
                }

                return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            throw new NotImplementedException("Bu grant type henüz desteklenmiyor.");
        }

        // Yardımcı Metot: Claim'lerin Access Token'a mı yoksa Identity Token'a mı gideceğini belirler
        private static IEnumerable<string> GetDestinations(Claim claim)
        {
            // Servisler arası iletişimde genellikle sadece Access Token yeterlidir.
            yield return Destinations.AccessToken;
        }

        // Helper Metot 2: Kullanıcı (Password/Refresh Grant) için detaylı versiyon
        private static IEnumerable<string> GetDestinations(Claim claim, ClaimsPrincipal principal)
        {
            // 1. Kural: Her claim MUTLAKA Access Token'a gitmeli (API'nin yetki kontrolü için)
            yield return Destinations.AccessToken;

            // 2. Kural: Kimlik bilgileri Identity Token'a da gitmeli (Frontend'in kullanıcıyı tanıması için)
            switch (claim.Type)
            {
                case Claims.Name:       // "name"
                case Claims.Email:      // "email"
                case Claims.Subject:    // "sub" (Hata almamak için bu şart!)
                case Claims.Role:       // "role"
                case "client_id":                           // Özel claimlerimiz
                    yield return Destinations.IdentityToken;
                    yield break;
            }
        }
        #endregion
        //[HttpPost("~/connect/token")]
        //[Produces("application/json")]
        //public async Task<IActionResult> Token()
        //{
        //    // OpenIddict'in /connect/token isteğini al
        //    var request = HttpContext.GetOpenIddictServerRequest()
        //        ?? throw new InvalidOperationException("OpenIddict isteği bulunamadı.");

        //    if (!request.IsPasswordGrantType())
        //        throw new InvalidOperationException("Sadece Password Grant Type destekleniyor.");

        //    // 1. Identity Kullanıcısını Bul
        //    var user = await _userManager.FindByNameAsync(request.Username);
        //    if (user == null)
        //        return Forbid(authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        //    // 2. Şifreyi Kontrol Et
        //    if (!await _userManager.CheckPasswordAsync(user, request.Password))
        //        return Forbid(authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        //    // 3. Claims (Token'a eklenecek bilgiler)
        //    var claims = new List<Claim>
        //    {
        //        // sub (Subject) claim'i, IdentityId'mizdir.
        //        new Claim(OpenIddictConstants.Claims.Subject, user.Id.ToString())
        //        // (Diğer claim'ler: Name, Email, Role vb.)
        //    };

        //    var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme));

        //    // Token bas ve dön
        //    return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        //}

        public class TokenRequest
        {
            [FromForm(Name = "grant_type")]
            public string GrantType { get; set; } = "password"; // Varsayılan değer

            [FromForm(Name = "username")]
            public string? Username { get; set; }

            [FromForm(Name = "password")]
            public string? Password { get; set; }

            [FromForm(Name = "client_id")]
            public string? ClientId { get; set; }

            [FromForm(Name = "client_secret")]
            public string? ClientSecret { get; set; }

            [FromForm(Name = "refresh_token")]
            public string? RefreshToken { get; set; }
        }

        public class RegisterRequestDto
        {
            public string Email { get; set; }
            public string Password { get; set; }
            public string UserName { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }
    }
}
