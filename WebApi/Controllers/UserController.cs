using Application.Exceptions;
using Application.Features.Users.Commands.SetUserCurrentBranch;
using Application.Features.Users.Commands.UpdateMyProfile;
using Application.Features.Users.DTOs;
using Application.Features.Users.Queries;
using Application.Features.Users.Queries.GetMyProfile;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
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
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateMyProfileRequest request)
        {
            // Güvenlik için token'dan alınan ID'leri kullanıyoruz
            var (userId, identityId) = await GetUserIdsFromToken();
            if (userId == Guid.Empty)
                return Unauthorized("Geçersiz token.");

            var command = new UpdateMyProfileCommand
            {
                UserId = userId,
                IdentityId = identityId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                FileId = request.FileId
            };

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
        /// Giriş yapan kullanıcının bir şubeye check-in yapmasını (veya ayrılmasını) sağlar.
        /// </summary>
        [HttpPut("me/check-in")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> CheckInToBranch([FromBody] SetUserBranchRequest request)
        {
            var (userId, _) = await GetUserIdsFromToken();
            if (userId == Guid.Empty)
                return Unauthorized("Geçersiz token veya profil bulunamadı.");

            var command = new SetUserCurrentBranchCommand
            {
                UserId = userId, // Sadece lokal ID'ye ihtiyacımız var
                NewBranchId = request.BranchId
            };

            await _sender.Send(command);
            return NoContent();
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

        #region Models
        public class UpdateMyProfileRequest
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string? FileId { get; set; }
        }

        public class SetUserBranchRequest
        {
            public Guid? BranchId { get; set; }
        }
        #endregion
    }
}
