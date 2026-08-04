using Application.Abstractions.Messaging;
using Domain.Enums;
using System.Text.Json.Serialization;

namespace Application.Features.ChatRoomInvites.Commands.CreateChatRoomInvite
{
    public class CreateChatRoomInviteCommand : ICommand<Guid>
    {
        public Guid InviteeUserId { get; set; }

        /// <summary>Private = geo'suz 1:1; Group = geo'lu.</summary>
        public RoomType TargetRoomType { get; set; } = RoomType.Private;

        [JsonIgnore]
        public Guid InviterUserId { get; set; }
        [JsonIgnore]
        public Guid UserCurrentBranchId { get; set; }
        [JsonIgnore]
        public Guid PublicChatRoomId { get; set; }
    }
}
