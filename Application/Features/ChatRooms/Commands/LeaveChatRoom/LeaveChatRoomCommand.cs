using Application.Abstractions.Messaging;

namespace Application.Features.ChatRooms.Commands.LeaveChatRoom
{
    public class LeaveChatRoomCommand : ICommand
    {
        public Guid RoomId { get; set; }

        [JsonIgnore]
        public Guid UserId { get; set; } // Token'dan
    }
}
