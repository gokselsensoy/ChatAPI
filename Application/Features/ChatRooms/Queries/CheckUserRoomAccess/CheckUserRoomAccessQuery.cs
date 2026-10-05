using MediatR;

namespace Application.Features.ChatRooms.Queries.CheckUserRoomAccess
{
    public class CheckUserRoomAccessQuery : IRequest<bool>
    {
        public Guid UserId { get; set; }
        public Guid RoomId { get; set; }
    }
}