using Application.Features.ChatRoomInvites.Commands.AcceptChatRoomInvite;
using Application.Features.ChatRoomInvites.Commands.CreateChatRoomInvite;
using Application.Features.ChatRoomInvites.Commands.DeclineChatRoomInvite;
using Application.Features.Users.DTOs;
using Application.Features.Users.Queries.GetMyProfile;
using Domain.Enums;
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
    [Route("api/chat-invites")]
    public class ChatRoomInviteController : ControllerBase
    {
        private readonly ISender _sender;
        public ChatRoomInviteController(ISender sender) { _sender = sender; }

        /// <summary>
        /// Public odadan Private (geo'suz 1:1) veya Group (geo'lu) daveti.
        /// </summary>
        [HttpPost("public-room/{publicRoomId:guid}")]
        public async Task<IActionResult> CreateInvite(Guid publicRoomId, [FromBody] CreateInviteRequest request)
        {
            var user = await GetMyProfileDto();
            if (user?.BranchId == null)
                return BadRequest("Davet göndermek için bir şubede olmalısınız.");

            var target = request.TargetRoomType;
            if (target is not (RoomType.Private or RoomType.Group))
                return BadRequest("targetRoomType yalnızca Private veya Group olabilir.");

            var inviteId = await _sender.Send(new CreateChatRoomInviteCommand
            {
                InviteeUserId = request.InviteeUserId,
                TargetRoomType = target,
                InviterUserId = user.Id,
                UserCurrentBranchId = user.BranchId.Value,
                PublicChatRoomId = publicRoomId
            });

            return Ok(new { InviteId = inviteId });
        }

        [HttpPost("accept/{inviteId:guid}")]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
        public async Task<IActionResult> AcceptInvite(Guid inviteId)
        {
            var user = await GetMyProfileDto();
            if (user == null) return Unauthorized();

            var newRoomId = await _sender.Send(new AcceptChatRoomInviteCommand
            {
                InviteId = inviteId,
                InviteeUserId = user.Id
            });

            return Ok(new { NewRoomId = newRoomId });
        }

        [HttpPost("decline/{inviteId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeclineInvite(Guid inviteId)
        {
            var user = await GetMyProfileDto();
            if (user == null) return Unauthorized();

            await _sender.Send(new DeclineChatRoomInviteCommand
            {
                InviteId = inviteId,
                InviteeUserId = user.Id
            });

            return NoContent();
        }

        private async Task<UserDto?> GetMyProfileDto()
        {
            var identityIdString = User.FindFirstValue(OpenIddictConstants.Claims.Subject)
                ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");

            if (string.IsNullOrEmpty(identityIdString) || !Guid.TryParse(identityIdString, out var identityId))
                return null;

            try
            {
                return await _sender.Send(new GetMyProfileQuery { IdentityId = identityId });
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    public class CreateInviteRequest
    {
        public Guid InviteeUserId { get; set; }

        /// <summary>Private = geo'suz 1:1; Group = geo'lu. Varsayılan Private.</summary>
        public RoomType TargetRoomType { get; set; } = RoomType.Private;
    }
}
