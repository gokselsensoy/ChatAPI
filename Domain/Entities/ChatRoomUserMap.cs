using Domain.SeedWork;

namespace Domain.Entities
{
    public class ChatRoomUserMap : Entity
    {
        public Guid ChatRoomId { get; private set; }
        public Guid UserId { get; private set; }
        public DateTime JoinedAt { get; private set; }
        public DateTime? LastReadAt { get; private set; }
        public DateTime? ClearedAt { get; private set; }
        public bool IsMuted { get; private set; }

        // Navigations
        public ChatRoom? ChatRoom { get; private set; }
        public User? User { get; private set; }

        private ChatRoomUserMap() { }

        public static ChatRoomUserMap Create(Guid chatRoomId, Guid userId)
        {
            var now = DateTime.UtcNow;
            return new ChatRoomUserMap
            {
                Id = Guid.NewGuid(),
                ChatRoomId = chatRoomId,
                UserId = userId,
                JoinedAt = now,
                LastReadAt = now,
                ClearedAt = null,
                IsMuted = false
            };
        }

        public void MarkAsRead()
        {
            LastReadAt = DateTime.UtcNow;
        }

        public void ClearHistory()
        {
            var now = DateTime.UtcNow;
            ClearedAt = now;
            LastReadAt = now;
            UpdatedDate = now;
        }

        public void SetMuted(bool isMuted)
        {
            IsMuted = isMuted;
            UpdatedDate = DateTime.UtcNow;
        }
    }
}