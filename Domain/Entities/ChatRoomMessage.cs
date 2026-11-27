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
        public ICollection<MessageReaction> Reactions {get; private set;} = new List<MessageReaction>();

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

        public (bool IsAdded, string? RemovedEmoji) ToggleReaction(Guid userId, string emoji)
        {
            var existingReaction = Reactions.FirstOrDefault(r => r.UserId == userId);

            // 1. Kullanıcının bu mesajda hiç reaksiyonu yoksa: Yeni ekle
            if (existingReaction == null)
            {
                Reactions.Add(MessageReaction.Create(Id, userId, emoji));
                return (true, null);
            }

            // 2. Kullanıcının reaksiyonu aynı emoji ise: Reaksiyonu kaldır (Toggle Off)
            if (existingReaction.Emoji == emoji)
            {
                Reactions.Remove(existingReaction);
                return (false, null);
            }

            // 3. Kullanıcının reaksiyonu farklı bir emoji ise: Eski emojiyi yenisiyle değiştir (Swap)
            string previousEmoji = existingReaction.Emoji;
            existingReaction.UpdateEmoji(emoji);
            return (true, previousEmoji);
        }

        public void AddReaction(Guid userId, string emoji)
        {
            ToggleReaction(userId, emoji);
        }

        public void RemoveReaction(Guid userId, string emoji)
        {
            var reaction = Reactions.FirstOrDefault(r => r.UserId == userId && r.Emoji == emoji);
            if(reaction != null)
            {
                Reactions.Remove(reaction);
            }
        }
    }
}
