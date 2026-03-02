using Application.Abstractions.Messaging;
using Application.Features.ChatRooms.DTOs;
using MediatR;

namespace Application.Features.ChatRooms.Queries.GetPublicRoomsByBranch
{
    public class GetPublicRoomsByBranchQuery : IRequest<List<ChatRoomDto>>
    {
        public Guid BranchId { get; set; }
    }
}
