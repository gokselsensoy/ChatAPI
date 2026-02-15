using Application.Features.Users.Commands;
using Application.Features.Users.Commands.CreateUser;
using Application.Features.Users.DTOs;
using Domain.Enums;
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            // 1. Transaction Başlat
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // 2. Identity User Oluştur
                var identityUser = new ApplicationUser
                {
                    Email = request.Email,
                    UserName = request.UserName,
                    EmailConfirmed = true
                };

                var identityResult = await _userManager.CreateAsync(identityUser, request.Password);
                if (!identityResult.Succeeded)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    // Hataları daha okunaklı dönelim
                    return BadRequest(new { Errors = identityResult.Errors.Select(e => e.Description) });
                }

                // 3. Domain User (Profil) Oluştur
                var command = new CreateUserCommand
                {
                    IdentityId = identityUser.Id,
                    UserName = identityUser.UserName,
                    Email = identityUser.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    UserType = request.UserType
                };

                var userId = await _sender.Send(command);

                // 4. Her şey yolundaysa kaydet
                await _unitOfWork.CommitTransactionAsync();

                // 5. Başarılı Dönüş
                // Token dönmüyoruz, client'a "Git /connect/token ile giriş yap" diyoruz.
                return Ok(new
                {
                    Message = "Kayıt başarılı. Giriş yapabilirsiniz.",
                    UserId = userId
                });
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return BadRequest(new { Error = "Kayıt sırasında bir hata oluştu.", Details = ex.Message });
            }
        }

        /// <summary>
        // OpenIddict Token Endpoint'i (/connect/token)
        // Bu metot, OpenIddict'in Password Flow'unu tetikler.
        /// </summary>
        #region connect/token
        [HttpPost("~/connect/token")]
        [Consumes("application/x-www-form-urlencoded")]
        [Produces("application/json")]
        public async Task<IActionResult> Exchange([FromForm] TokenRequest request)
        {
            var openIddictRequest = HttpContext.GetOpenIddictServerRequest();
            if (openIddictRequest == null) throw new InvalidOperationException("OpenID isteği okunamadı.");

            // 1. SENARYO: KULLANICI (Mobil/Web - Password Grant)
            if (openIddictRequest.IsPasswordGrantType())
            {
                var user = await _userManager.FindByNameAsync(openIddictRequest.Username);
                if (user == null) return InvalidGrant("Kullanıcı adı veya şifre hatalı.");

                var result = await _signInManager.CheckPasswordSignInAsync(user, openIddictRequest.Password, lockoutOnFailure: true);
                if (!result.Succeeded) return InvalidGrant("Kullanıcı adı veya şifre hatalı.");

                if (_userManager.Options.SignIn.RequireConfirmedEmail && !await _userManager.IsEmailConfirmedAsync(user))
                {
                    return InvalidGrant("Lütfen önce email adresinizi onaylayın.");
                }

                // Yardımcı metodu çağırıyoruz (Scope'ları buradan geçiriyoruz)
                var principal = await CreateUserPrincipalAsync(user, openIddictRequest.GetScopes());

                return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            // 2. SENARYO: REFRESH TOKEN
            if (openIddictRequest.IsRefreshTokenGrantType())
            {
                var info = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                var user = await _userManager.GetUserAsync(info.Principal);

                if (user == null || !await _signInManager.CanSignInAsync(user))
                {
                    return InvalidGrant("Refresh token artık geçerli değil.");
                }

                // Refresh token'dan gelen scope'ları koruyarak yeni token üret
                var principal = await CreateUserPrincipalAsync(user, info.Principal.GetScopes());

                return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            throw new NotImplementedException("Bu grant type henüz desteklenmiyor.");
        }

        // --- YARDIMCI METOTLAR (Token Oluşturma Mantığı Burada Toplandı) ---

        private async Task<ClaimsPrincipal> CreateUserPrincipalAsync(ApplicationUser user, IEnumerable<string> scopes)
        {
            var principal = await _signInManager.CreateUserPrincipalAsync(user);
            var identity = (ClaimsIdentity)principal.Identity;

            // *** DÜZELTME BAŞLANGICI ***
            // Sorunun Çözümü: OpenIddict "sub" claim'ini zorunlu tutar.
            // Eğer Identity bu claim'i oluşturmadıysa (ki varsayılan olarak oluşturmaz), biz ekliyoruz.
            if (!principal.HasClaim(c => c.Type == OpenIddictConstants.Claims.Subject))
            {
                var userId = await _userManager.GetUserIdAsync(user);
                identity.AddClaim(new Claim(OpenIddictConstants.Claims.Subject, userId));
            }
            // *** DÜZELTME BİTİŞİ ***

            // Client'tan gelen scope'ları (örn: offline_access) token'a işle
            principal.SetScopes(scopes);

            // Her claim'in nereye gideceğini (Access Token mı Identity Token mı) belirle
            foreach (var claim in principal.Claims)
            {
                claim.SetDestinations(GetDestinations(claim));
            }

            return principal;
        }

        private static IEnumerable<string> GetDestinations(Claim claim)
        {
            // Tüm claimler Access Token'a gitsin (API yetki kontrolü için)
            yield return Destinations.AccessToken;

            // Bazı önemli claimler Identity Token'a da gitsin (Frontend profili görsün diye)
            switch (claim.Type)
            {
                case Claims.Name:
                case Claims.Email:
                case Claims.Subject: // User ID
                case Claims.Role:
                    yield return Destinations.IdentityToken;
                    yield break;
            }
        }

        private IActionResult InvalidGrant(string message)
        {
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = message
                }));
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

            [FromForm(Name = "scope")]
            public string? Scope { get; set; } // "offline_access"
        }

        public class RegisterRequestDto
        {
            public string Email { get; set; }
            public string Password { get; set; }
            public string UserName { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public UserType UserType { get; set; }
        }
    }
}
