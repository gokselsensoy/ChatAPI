using Application.Features.Users.Commands.SetUserCurrentBranch;
using Application.Features.Users.DTOs;
using Application.Features.Users.Queries;
using Application.Features.Users.Queries.GetMyProfile;
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
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CheckInToBranch([FromBody] SetUserBranchRequest request)
        {
            var identityIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(identityIdString) || !Guid.TryParse(identityIdString, out var identityId))
            {
                return Unauthorized("Geçersiz token.");
            }

            // Önce lokal profili bulmamız lazım
            var userDto = await _sender.Send(new GetMyProfileQuery { IdentityId = identityId });

            var command = new SetUserCurrentBranchCommand
            {
                UserId = userDto.Id, // Handler'ın lokal ID'ye (PKey) ihtiyacı var
                NewBranchId = request.BranchId
            };

            await _sender.Send(command);
            return NoContent();
        }
    }

    public class SetUserBranchRequest
    {
        public Guid? BranchId { get; set; }
    }
}
