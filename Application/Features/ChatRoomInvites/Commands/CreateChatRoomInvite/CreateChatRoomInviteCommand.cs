using Application.Abstractions.Messaging;
using System.Text.Json.Serialization;

namespace Application.Features.ChatRoomInvites.Commands.CreateChatRoomInvite
{
    public class CreateChatRoomInviteCommand : ICommand<Guid>
    {
        public Guid InviteeUserId { get; set; } // Kime davet atılıyor?

        [JsonIgnore]
        public Guid InviterUserId { get; set; } // Token'dan
        [JsonIgnore]
        public Guid UserCurrentBranchId { get; set; } // Token'dan
        [JsonIgnore]
        public Guid PublicChatRoomId { get; set; } // Hangi odadan davet atıyor?
    }
}
