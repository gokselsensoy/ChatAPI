using MediatR;

namespace Application.Features.ChatRooms.Commands.ToggleChatMute
{
    public class ToggleChatMuteCommand : IRequest<bool>
    {
        public Guid RoomId { get; set; }
        public Guid UserId { get; set; }
        public bool IsMuted { get; set; }
    }
}
