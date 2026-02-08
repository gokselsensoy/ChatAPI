using Application.Exceptions;
using Application.Features.Users.Commands.CheckIn;
using Application.Features.Users.Commands.CheckOut;
using Application.Features.Users.Commands.CheckOutControl;
using Application.Features.Users.Commands.UpdateMyProfile;
using Application.Features.Users.DTOs;
using Application.Features.Users.Queries.GetMyProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/users")]
    //[Authorize]
    public class UsersController : ControllerBase
    {
        private readonly ISender _sender;

        public UsersController(ISender sender)
        {
            _sender = sender;
        }

        /// <summary>
        /// Giriş yapan kullanıcının profil bilgilerini (isim, soyisim, fotoğraf) günceller.
        /// </summary>
        [HttpPut("me")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateMyProfileCommand command)
        {
            // 1. Token'dan ID'leri al
            var (userId, identityId) = await GetUserIdsFromToken();
            if (userId == Guid.Empty)
                return Unauthorized("Geçersiz token.");

            command.UserId = userId;
            command.IdentityId = identityId;

            await _sender.Send(command);

            return NoContent();
        }

        /// <summary>
        /// Giriş yapan kullanıcının profilini getirir.
        /// </summary>
        [HttpGet("me")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMyProfile()
        {
            // Token'dan 'sub' claim'ini oku (Bu bizim IdentityId'miz)
            var identityIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(identityIdString) || !Guid.TryParse(identityIdString, out var identityId))
            {
                return Unauthorized("Geçersiz token (IdentityId bulunamadı).");
            }

            var query = new GetMyProfileQuery { IdentityId = identityId };
            var userDto = await _sender.Send(query);

            return Ok(userDto);
        }

        /// <summary>
        /// Kullanıcı bir şubeye giriş yapar (Check-In).
        /// </summary>
        [HttpPost("check-in")]
        public async Task<IActionResult> CheckIn([FromBody] CheckInCommand command)
        {
            command.UserId = GetCurrentUserId();
            await _sender.Send(command);
            return Ok(new { Message = "Check-in başarılı." });
        }

        /// <summary>
        /// Kullanıcı mekandan manuel olarak ayrılır (Check-Out).
        /// </summary>
        [HttpPost("check-out")]
        public async Task<IActionResult> CheckOut()
        {
            // Body'den veri almaya gerek yok, sadece tetiklemek yeterli.
            var command = new CheckOutCommand
            {
                UserId = GetCurrentUserId()
            };

            await _sender.Send(command);
            return Ok(new { Message = "Çıkış işlemi başarılı." });
        }

        /// <summary>
        /// Mobil uygulama arka planda bu endpoint'i çağırır.
        /// Eğer kullanıcı 100m uzaklaşmışsa otomatik check-out yapılır.
        /// </summary>
        [HttpPost("check-out-control")]
        public async Task<IActionResult> CheckOutControl([FromBody] HeartbeatRequest request)
        {
            var command = new CheckOutControlCommand
            {
                UserId = GetCurrentUserId(),
                Latitude = request.Latitude,
                Longitude = request.Longitude
            };

            bool isCheckedOut = await _sender.Send(command);

            if (isCheckedOut)
            {
                return Ok(new { Status = "CheckedOut", Message = "Mekandan uzaklaştığınız için çıkış yapıldı." });
            }

            return Ok(new { Status = "Active", Message = "Konum güncellendi, hala mekandasınız." });
        }

        // --- Yardımcı Metot: Token'dan User ID Okuma ---
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("Geçersiz token. Kullanıcı ID bulunamadı.");
            }

            return userId;
        }

        public class HeartbeatRequest
        {
            public decimal Latitude { get; set; }
            public decimal Longitude { get; set; }
        }

        #region Helper Methods
        private async Task<(Guid UserId, Guid IdentityId)> GetUserIdsFromToken()
        {
            var identityIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(identityIdString) || !Guid.TryParse(identityIdString, out var identityId))
            {
                return (Guid.Empty, Guid.Empty);
            }

            try
            {
                var userDto = await _sender.Send(new GetMyProfileQuery { IdentityId = identityId });
                return (userDto.Id, userDto.IdentityId);
            }
            catch (NotFoundException)
            {
                return (Guid.Empty, Guid.Empty);
            }
        }
        #endregion
    }
}
