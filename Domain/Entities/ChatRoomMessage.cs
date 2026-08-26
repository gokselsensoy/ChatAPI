using Domain.Exceptions;
using Domain.SeedWork;

namespace Domain.Entities
{
    public class ChatRoomMessage : Entity
    {
        public string Message { get; private set; }
        public Guid ChatRoomId { get; private set; }
        public Guid SenderUserId { get; private set; }
        public Guid? ReplyToMessageId { get; private set; }

        // Navigations
        public ChatRoom? ChatRoom { get; private set; }
        public User? SenderUser { get; private set; }
        public ChatRoomMessage? ReplyToMessage { get; private set; }

        private ChatRoomMessage() { }

        public static ChatRoomMessage Create(
            Guid chatRoomId,
            Guid senderUserId,
            string message,
            Guid? replyToMessageId = null)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ChatRoomDomainException("Mesaj boş olamaz.");

            return new ChatRoomMessage
            {
                Id = Guid.NewGuid(),
                ChatRoomId = chatRoomId,
                SenderUserId = senderUserId,
                Message = message,
                ReplyToMessageId = replyToMessageId,
                CreatedDate = DateTime.UtcNow
            };
        }
    }
}
