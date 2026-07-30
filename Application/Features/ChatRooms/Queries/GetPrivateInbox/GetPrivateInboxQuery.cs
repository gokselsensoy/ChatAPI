using Application.Features.ChatRooms.DTOs;
using MediatR;

namespace Application.Features.ChatRooms.Queries.GetPrivateInbox
{
    public class GetPrivateInboxQuery : IRequest<List<ChatRoomDto>>
    {
        public Guid UserId { get; set; }
    }
}
