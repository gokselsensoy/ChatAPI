using Application.Features.ChatRooms.Commands.CreateChatRoom;
using Application.Features.ChatRooms.Commands.JoinChatRoom;
using Application.Features.ChatRooms.Commands.LeaveChatRoom;
using Application.Features.ChatRooms.Commands.SendMessage;
using Application.Features.ChatRooms.DTOs;
using Application.Features.ChatRooms.Queries.GetChatRoomMessages;
using Application.Features.ChatRooms.Queries.GetPublicRoomsByBranch;
using Application.Features.Users.DTOs;
using Application.Features.Users.Queries.GetMyProfile;
using Application.Shared.Pagination;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/chatrooms")]
    [Authorize] // Tüm Chat işlemleri üyelik gerektirir
    public class ChatRoomController : ControllerBase
    {
        private readonly ISender _sender;

        public ChatRoomController(ISender sender)
        {
            _sender = sender;
        }

        /// <summary>
        /// Kullanıcının o an check-in yaptığı şubedeki PUBLIC odaları listeler.
        /// </summary>
        [HttpGet("public")]
        [ProducesResponseType(typeof(List<ChatRoomDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPublicRoomsForMyBranch()
        {
            var user = await GetMyProfileDto();
            if (user?.BranchId == null)
                return BadRequest("Oda listelemek için önce bir şubeye check-in yapmalısınız.");

            var query = new GetPublicRoomsByBranchQuery { BranchId = user.BranchId.Value };
            // CachingPipelineBehaviour devreye girecek
            var rooms = await _sender.Send(query);
            return Ok(rooms);
        }

        /// <summary>
        /// O anki şubede yeni bir chat odası oluşturur. (Genelde admin yetkisi gerektirir)
        /// </summary>
        [HttpPost]
        // [Authorize(Roles = "Admin")] // TODO: Yetkilendirme eklenecek
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateRoom([FromBody] CreateChatRoomCommand command)
        {
            var user = await GetMyProfileDto();
            if (user?.BranchId == null)
                return BadRequest("Oda oluşturmak için önce bir şubeye check-in yapmalısınız.");

            command.BranchId = user.BranchId.Value;

            var roomId = await _sender.Send(command);
            return CreatedAtAction(nameof(GetMessages), new { roomId = roomId }, new { id = roomId });
        }

        /// <summary>
        /// Belirtilen odaya katılır.
        /// </summary>
        [HttpPost("{roomId:guid}/join")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> JoinRoom(Guid roomId)
        {
            var user = await GetMyProfileDto();
            if (user?.BranchId == null)
                return BadRequest("Odaya katılmak için önce bir şubeye check-in yapmalısınız.");

            var command = new JoinChatRoomCommand
            {
                RoomId = roomId,
                UserId = user.Id,
                UserCurrentBranchId = user.BranchId.Value
            };

            await _sender.Send(command);
            return NoContent();
        }

        /// <summary>
        /// Belirtilen odadan ayrılır.
        /// </summary>
        [HttpPost("{roomId:guid}/leave")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> LeaveRoom(Guid roomId)
        {
            var user = await GetMyProfileDto();
            if (user == null) return Unauthorized();

            var command = new LeaveChatRoomCommand
            {
                RoomId = roomId,
                UserId = user.Id
            };

            await _sender.Send(command);
            return NoContent();
        }

        /// <summary>
        /// Bir odadaki mesajları sayfalı olarak listeler (Cache'lenmez).
        /// </summary>
        [HttpGet("{roomId:guid}/messages")]
        [ProducesResponseType(typeof(PaginatedResponse<ChatRoomMessageDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMessages(Guid roomId, [FromQuery] PaginatedRequest pagination)
        {
            var query = new GetChatMessagesQuery
            {
                RoomId = roomId,
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize
            };
            var messages = await _sender.Send(query);
            return Ok(messages);
        }

        /// <summary>
        /// Bir odaya mesaj gönderir (SignalR ile yayınlanır).
        /// </summary>
        [HttpPost("{roomId:guid}/messages")]
        [ProducesResponseType(typeof(ChatRoomMessageDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> SendMessage(Guid roomId, [FromBody] SendMessageRequest request)
        {
            var user = await GetMyProfileDto();
            if (user == null) return Unauthorized();

            var command = new SendMessageCommand
            {
                RoomId = roomId,
                Message = request.Message,
                SenderUserId = user.Id,
                SenderUserName = user.UserName // DTO için
            };

            var messageDto = await _sender.Send(command);

            // DTO'yu 201 Created ile geri dön
            return CreatedAtAction(nameof(GetMessages), new { roomId = roomId }, messageDto);
        }

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

    // SendMessage için basit request DTO
    public class SendMessageRequest
    {
        public string Message { get; set; }
    }
}
