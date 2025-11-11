using Application.Abstractions.Messaging;
using System.Text.Json.Serialization;

namespace Application.Features.ChatRooms.Commands.JoinChatRoom
{
    public class JoinChatRoomCommand : ICommand
    {
        public Guid RoomId { get; set; }

        [JsonIgnore]
        public Guid UserId { get; set; } // Token'dan
        [JsonIgnore]
        public Guid UserCurrentBranchId { get; set; } // Token'dan (kontrol için)
    }
}
