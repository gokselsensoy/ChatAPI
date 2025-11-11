using Application.Features.ChatRooms.DTOs;
using Application.Shared.Pagination;
using MediatR;

namespace Application.Features.ChatRooms.Queries.GetChatRoomMessages
{
    public class GetChatMessagesQuery : PaginatedRequest, IRequest<PaginatedResponse<ChatRoomMessageDto>>
    {
        public Guid RoomId { get; set; }
    }
}
