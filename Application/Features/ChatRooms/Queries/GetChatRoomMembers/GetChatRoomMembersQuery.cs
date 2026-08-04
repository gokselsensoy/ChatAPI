using Application.Features.ChatRooms.DTOs;
using MediatR;

namespace Application.Features.ChatRooms.Queries.GetChatRoomMembers
{
    public class GetChatRoomMembersQuery : IRequest<List<ChatRoomMemberDto>>
    {
        public Guid RoomId { get; set; }
        public Guid RequestingUserId { get; set; }
    }
}
