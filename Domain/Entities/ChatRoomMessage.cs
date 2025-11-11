using Domain.SeedWork;

namespace Domain.Entities
{
    public class ChatRoomMessage : Entity
    {
        public string Message { get; private set; }
        public Guid ChatRoomId { get; private set; }
        public Guid SenderUserId { get; private set; }

        // Navigations
        public ChatRoom? ChatRoom { get; private set; }
        public User? SenderUser { get; private set; }

        private ChatRoomMessage() { }

        public static ChatRoomMessage Create(Guid chatRoomId, Guid senderUserId, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new Exception("Mesaj boş olamaz."); // (ChatRoomDomainException)

            return new ChatRoomMessage
            {
                Id = Guid.NewGuid(),
                ChatRoomId = chatRoomId,
                SenderUserId = senderUserId,
                Message = message,
                CreatedDate = DateTime.UtcNow // (Base 'Entity'de bu yoksa buraya ekle)
            };
        }
    }
}