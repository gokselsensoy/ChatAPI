using Application.Abstractions.Messaging;
using System.Text.Json.Serialization;

namespace Application.Features.ChatRooms.Commands.CreateGroupRoom
{
    public class CreateGroupRoomCommand : ICommand<Guid>
    {
        public string Name { get; set; }
        public List<Guid> UserIds { get; set; }

        [JsonIgnore]
        public Guid CreatorUserId { get; set; }
        [JsonIgnore]
        public Guid BranchId { get; set; }
    }
}
