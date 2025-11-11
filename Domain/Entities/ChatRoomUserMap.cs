using Domain.SeedWork;

namespace Domain.Entities
{
    public class ChatRoomUserMap : Entity
    {
        public Guid ChatRoomId { get; private set; }
        public Guid UserId { get; private set; }
        public DateTime JoinedAt { get; private set; }

        // Navigations
        public ChatRoom? ChatRoom { get; private set; }
        public User? User { get; private set; }

        private ChatRoomUserMap() { }

        public static ChatRoomUserMap Create(Guid chatRoomId, Guid userId)
        {
            return new ChatRoomUserMap
            {
                Id = Guid.NewGuid(),
                ChatRoomId = chatRoomId,
                UserId = userId,
                JoinedAt = DateTime.UtcNow
            };
        }
    }
}