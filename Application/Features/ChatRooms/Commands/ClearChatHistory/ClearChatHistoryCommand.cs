using MediatR;

namespace Application.Features.ChatRooms.Commands.ClearChatHistory
{
    public class ClearChatHistoryCommand : IRequest
    {
        public Guid RoomId { get; set; }
        public Guid UserId { get; set; }
    }
}
