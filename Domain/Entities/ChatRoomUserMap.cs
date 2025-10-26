using Domain.SeedWork;

namespace Domain.Entities
{
    public class ChatRoomUserMap : Entity
    {
        public Guid ChatRoomId { get; private set; }
        public Guid UserId { get; private set; }

        // Navigations
        public ChatRoom? ChatRoom { get; private set; }
        public User? User { get; private set; }

        private ChatRoomUserMap() { }
    }
}