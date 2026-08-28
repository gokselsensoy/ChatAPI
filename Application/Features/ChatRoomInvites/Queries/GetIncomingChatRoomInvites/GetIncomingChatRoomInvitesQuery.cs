using Application.Features.ChatRoomInvites.DTOs;
using MediatR;

namespace Application.Features.ChatRoomInvites.Queries.GetIncomingChatRoomInvites
{
    public class GetIncomingChatRoomInvitesQuery : IRequest<List<IncomingChatRoomInviteDto>>
    {
        public Guid InviteeUserId { get; set; }
    }
}
