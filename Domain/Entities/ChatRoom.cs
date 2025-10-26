using Domain.Enums;
using Domain.SeedWork;

namespace Domain.Entities
{
    public class ChatRoom : Entity, IAggregateRoot
    {
        public string Name { get; private set; }
        public RoomType RoomType { get; private set; }
        public Guid BranchId { get; private set; }

        // Navigations
        public Branch? Branch { get; private set; }
        public ICollection<ChatRoomMessages> Messages { get; private set; } = new List<ChatRoomMessages>();
        public ICollection<ChatRoomUserMap> ChatRoomUserMaps { get; private set; } = new List<ChatRoomUserMap>();
        public ICollection<ChatRoomInvite> ChatRoomInvites { get; private set; } = new List<ChatRoomInvite>();

        private ChatRoom() { }
    }
}