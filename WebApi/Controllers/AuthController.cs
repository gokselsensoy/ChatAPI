using Application.Features.Users.Commands;
using Application.Features.Users.Commands.CreateUser;
using Application.Features.Users.DTOs;
using Domain.SeedWork;
using Infrastructure.Identity.Models;
using MediatR;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ISender _sender;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public AuthController(ISender sender, UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork)
        {
            _sender = sender;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Yeni bir kullanıcı kaydı oluşturur (Identity + Lokal Profil).
        /// </summary>
        [HttpPost("api/auth/register")]
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
        [HttpPost("~/connect/token")]
        [Produces("application/json")]
        public async Task<IActionResult> Token()
        {
            // OpenIddict'in /connect/token isteğini al
            var request = HttpContext.GetOpenIddictServerRequest()
                ?? throw new InvalidOperationException("OpenIddict isteği bulunamadı.");

            if (!request.IsPasswordGrantType())
                throw new InvalidOperationException("Sadece Password Grant Type destekleniyor.");

            // 1. Identity Kullanıcısını Bul
            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null)
                return Forbid(authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            // 2. Şifreyi Kontrol Et
            if (!await _userManager.CheckPasswordAsync(user, request.Password))
                return Forbid(authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            // 3. Claims (Token'a eklenecek bilgiler)
            var claims = new List<Claim>
            {
                // sub (Subject) claim'i, IdentityId'mizdir.
                new Claim(OpenIddictConstants.Claims.Subject, user.Id.ToString())
                // (Diğer claim'ler: Name, Email, Role vb.)
            };

            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme));

            // Token bas ve dön
            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
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
