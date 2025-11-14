using Application.Abstractions.Messaging;
using System.Text.Json.Serialization;

namespace Application.Features.ChatRoomInvites.Commands.AcceptChatRoomInvite
{
    public class AcceptChatRoomInviteCommand : ICommand<Guid> // Yeni özel odanın ID'sini döner
    {
        [JsonIgnore]
        public Guid InviteId { get; set; } // Route'dan
        [JsonIgnore]
        public Guid InviteeUserId { get; set; } // Token'dan (güvenlik)
    }
}
