using Application.Features.Devices.Commands.RegisterDeviceToken;
using Application.Features.Devices.Commands.UnregisterDeviceToken;
using Application.Features.Users.Queries.GetMyProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;
using System.Security.Claims;

namespace WebApi.Controllers
{
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/devices")]
    public class DevicesController : ControllerBase
    {
        private readonly ISender _sender;

        public DevicesController(ISender sender)
        {
            _sender = sender;
        }

        /// <summary>
        /// FCM device token kaydı / güncelleme (login ve token yenilemede çağır).
        /// </summary>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Register([FromBody] RegisterDeviceRequest body, CancellationToken cancellationToken)
        {
            var userId = await GetDomainUserIdAsync(cancellationToken);
            await _sender.Send(new RegisterDeviceTokenCommand
            {
                UserId = userId,
                Token = body.Token,
                Platform = body.Platform
            }, cancellationToken);

            return NoContent();
        }

        /// <summary>
        /// Logout veya token geçersizleşince cihaz kaydını siler.
        /// </summary>
        [HttpDelete("{token}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Unregister(string token, CancellationToken cancellationToken)
        {
            var userId = await GetDomainUserIdAsync(cancellationToken);
            await _sender.Send(new UnregisterDeviceTokenCommand
            {
                UserId = userId,
                Token = Uri.UnescapeDataString(token)
            }, cancellationToken);

            return NoContent();
        }

        private async Task<Guid> GetDomainUserIdAsync(CancellationToken cancellationToken)
        {
            var identityIdString = User.FindFirstValue(OpenIddictConstants.Claims.Subject)
                ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");

            if (string.IsNullOrEmpty(identityIdString) || !Guid.TryParse(identityIdString, out var identityId))
                throw new UnauthorizedAccessException("Geçersiz token.");

            var profile = await _sender.Send(new GetMyProfileQuery { IdentityId = identityId }, cancellationToken);
            return profile.Id;
        }
    }

    public class RegisterDeviceRequest
    {
        public string Token { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
    }
}
