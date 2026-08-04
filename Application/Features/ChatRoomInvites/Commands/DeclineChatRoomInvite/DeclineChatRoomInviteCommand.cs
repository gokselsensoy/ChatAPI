using Application.Abstractions.Messaging;
using System.Text.Json.Serialization;

namespace Application.Features.ChatRoomInvites.Commands.DeclineChatRoomInvite
{
    public class DeclineChatRoomInviteCommand : ICommand
    {
        [JsonIgnore]
        public Guid InviteId { get; set; }

        [JsonIgnore]
        public Guid InviteeUserId { get; set; }
    }
}
