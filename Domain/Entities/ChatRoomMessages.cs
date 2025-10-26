using Domain.SeedWork;

namespace Domain.Entities
{
    public class ChatRoomMessages : Entity
    {
        public string Message { get; private set; }
        public Guid ChatRoomId { get; private set; }
        public Guid SenderUserId { get; private set; }

        // Navigations
        public ChatRoom? ChatRoom { get; private set; }
        public User? SenderUser { get; private set; }

        private ChatRoomMessages() { }
    }
}