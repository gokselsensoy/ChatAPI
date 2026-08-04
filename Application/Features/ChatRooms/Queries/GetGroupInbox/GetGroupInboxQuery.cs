using Application.Features.ChatRooms.DTOs;
using MediatR;

namespace Application.Features.ChatRooms.Queries.GetGroupInbox
{
    public class GetGroupInboxQuery : IRequest<List<ChatRoomDto>>
    {
        public Guid UserId { get; set; }
    }
}
