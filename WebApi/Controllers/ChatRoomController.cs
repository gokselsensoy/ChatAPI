using Application.Features.ChatRooms.Commands.CreateChatRoom;
using Application.Features.ChatRooms.Commands.CreateGroupRoom;
using Application.Features.ChatRooms.Commands.JoinChatRoom;
using Application.Features.ChatRooms.Commands.LeaveChatRoom;
using Application.Features.ChatRooms.Commands.SendMessage;
using Application.Features.ChatRooms.DTOs;
using Application.Features.ChatRooms.Queries.GetChatRoomMessages;
using Application.Features.ChatRooms.Queries.GetGroupInbox;
using Application.Features.ChatRooms.Queries.GetPrivateInbox;
using Application.Features.ChatRooms.Queries.GetPublicRoomsByBranch;
using Application.Features.Users.DTOs;
using Application.Features.Users.Queries.GetMyProfile;
using Application.Shared.Pagination;
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
    [Route("api/chatrooms")]
    public class ChatRoomController : ControllerBase
    {
        private readonly ISender _sender;

        public ChatRoomController(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet("public")]
        [ProducesResponseType(typeof(List<ChatRoomDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPublicRoomsForMyBranch(CancellationToken cancellationToken)
        {
            var user = await GetMyProfileDto();
            if (user == null) return Unauthorized();

            var rooms = await _sender.Send(new GetPublicRoomsByBranchQuery { UserId = user.Id }, cancellationToken);
            return Ok(rooms);
        }

        /// <summary>
        /// Private 1:1 inbox (geo'suz) — peer + isOnline + unread.
        /// </summary>
        [HttpGet("private-inbox")]
        [ProducesResponseType(typeof(List<ChatRoomDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPrivateInbox(CancellationToken cancellationToken)
        {
            var user = await GetMyProfileDto();
            if (user == null) return Unauthorized();

            var rooms = await _sender.Send(new GetPrivateInboxQuery { UserId = user.Id }, cancellationToken);
            return Ok(rooms);
        }

        /// <summary>
        /// Group inbox (geo'lu masa / özel) — unread + onlineMemberCount.
        /// </summary>
        [HttpGet("group-inbox")]
        [ProducesResponseType(typeof(List<ChatRoomDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetGroupInbox(CancellationToken cancellationToken)
        {
            var user = await GetMyProfileDto();
            if (user == null) return Unauthorized();

            var rooms = await _sender.Send(new GetGroupInboxQuery { UserId = user.Id }, cancellationToken);
            return Ok(rooms);
        }

        /// <summary>Eski premium-inbox yolu — private-inbox ile aynı.</summary>
        [HttpGet("premium-inbox")]
        [Obsolete("Use GET /api/chatrooms/private-inbox")]
        [ProducesResponseType(typeof(List<ChatRoomDto>), StatusCodes.Status200OK)]
        public Task<IActionResult> GetPremiumInboxAlias(CancellationToken cancellationToken) =>
            GetPrivateInbox(cancellationToken);

        [HttpPost]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateRoom([FromBody] CreateChatRoomCommand command)
        {
            var user = await GetMyProfileDto();
            if (user?.BranchId == null)
                return BadRequest("Oda oluşturmak için bir şubeye check-in yapmalısınız.");

            command.BranchId = user.BranchId.Value;
            command.RoomType = RoomType.Public;

            var roomId = await _sender.Send(command);
            return CreatedAtAction(nameof(GetMessages), new { roomId = roomId }, new { id = roomId });
        }

        [HttpPost("join/{roomId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> JoinRoom(Guid roomId)
        {
            var user = await GetMyProfileDto();
            if (user == null) return Unauthorized();

            await _sender.Send(new JoinChatRoomCommand { RoomId = roomId, UserId = user.Id });
            return NoContent();
        }

        [HttpPost("leave/{roomId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> LeaveRoom(Guid roomId)
        {
            var user = await GetMyProfileDto();
            if (user == null) return Unauthorized();

            await _sender.Send(new LeaveChatRoomCommand { RoomId = roomId, UserId = user.Id });
            return NoContent();
        }

        [HttpGet("messages/{roomId:guid}")]
        [ProducesResponseType(typeof(PaginatedResponse<ChatRoomMessageDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMessages(Guid roomId, [FromQuery] PaginatedRequest pagination)
        {
            var messages = await _sender.Send(new GetChatMessagesQuery
            {
                RoomId = roomId,
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize
            });
            return Ok(messages);
        }

        [HttpPost("messages/{roomId:guid}")]
        [ProducesResponseType(typeof(ChatRoomMessageDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> SendMessage(Guid roomId, [FromBody] SendMessageRequest request)
        {
            var user = await GetMyProfileDto();
            if (user == null) return Unauthorized();

            var messageDto = await _sender.Send(new SendMessageCommand
            {
                RoomId = roomId,
                Message = request.Message,
                SenderUserId = user.Id,
                SenderUserName = user.UserName
            });

            return CreatedAtAction(nameof(GetMessages), new { roomId = roomId }, messageDto);
        }

        /// <summary>Çok kişilik Group (geo'lu) odası.</summary>
        [HttpPost("create-group-room")]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateGroupRoom([FromBody] CreateGroupRoomCommand command)
        {
            var user = await GetMyProfileDto();
            if (user?.BranchId == null)
                return BadRequest("Grup oluşturmak için önce bir şubeye check-in yapmalısınız.");

            command.CreatorUserId = user.Id;
            command.BranchId = user.BranchId.Value;

            var roomId = await _sender.Send(command);
            return CreatedAtAction(nameof(GetMessages), new { roomId = roomId }, new { id = roomId });
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

    public class SendMessageRequest
    {
        public string Message { get; set; } = string.Empty;
    }
}
