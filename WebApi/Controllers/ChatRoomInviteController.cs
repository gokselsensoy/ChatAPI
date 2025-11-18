using Application.Features.ChatRoomInvites.Commands.AcceptChatRoomInvite;
using Application.Features.ChatRoomInvites.Commands.CreateChatRoomInvite;
using Application.Features.Users.DTOs;
using Application.Features.Users.Queries.GetMyProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/chat-invites")]
    //[Authorize]
    public class ChatRoomInviteController : ControllerBase
    {
        private readonly ISender _sender;
        public ChatRoomInviteController(ISender sender) { _sender = sender; }

        /// <summary>
        /// Bir kullanıcıya 1-e-1 özel sohbet daveti gönderir.
        /// </summary>
        [HttpPost("public-room/{publicRoomId:guid}")]
        public async Task<IActionResult> CreateInvite(Guid publicRoomId, [FromBody] CreateInviteRequest request)
        {
            var user = await GetMyProfileDto();
            if (user?.BranchId == null)
                return BadRequest("Davet göndermek için bir şubede olmalısınız.");

            var command = new CreateChatRoomInviteCommand
            {
                InviteeUserId = request.InviteeUserId,
                InviterUserId = user.Id,
                UserCurrentBranchId = user.BranchId.Value,
                PublicChatRoomId = publicRoomId
            };

            var inviteId = await _sender.Send(command);
            return Ok(new { InviteId = inviteId });
        }

        /// <summary>
        /// Gelen bir daveti kabul eder ve yeni bir özel oda oluşturur.
        /// </summary>
        [HttpPost("{inviteId:guid}/accept")]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)] // Yeni oda ID'si
        public async Task<IActionResult> AcceptInvite(Guid inviteId)
        {
            var user = await GetMyProfileDto();
            if (user == null) return Unauthorized();

            var command = new AcceptChatRoomInviteCommand
            {
                InviteId = inviteId,
                InviteeUserId = user.Id // Güvenlik
            };

            var newRoomId = await _sender.Send(command);
            return Ok(new { NewRoomId = newRoomId });
        }

        // (Decline için de benzer bir endpoint eklenebilir)

        // --- Yardımcı Metot ---
        private async Task<UserDto?> GetMyProfileDto()
        {
            var identityIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(identityIdString) || !Guid.TryParse(identityIdString, out var identityId))
            {
                return null;
            }
            try
            {
                return await _sender.Send(new GetMyProfileQuery { IdentityId = identityId });
            }
            catch (Exception) { return null; }
        }
    }

    public class CreateInviteRequest
    {
        public Guid InviteeUserId { get; set; }
    }
}
