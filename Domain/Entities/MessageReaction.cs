using Domain.SeedWork;

namespace Domain.Entities
{
    public class MessageReaction : Entity
    {
        public Guid ChatRoomMessageId {get; private set;}
        public Guid UserId {get; private set;}
        public string Emoji {get; private set;}

        //Navigations
        public ChatRoomMessage? ChatRoomMessage {get; private set;}
        public User? User {get; private set;}

        private MessageReaction() {} //EF Core'un veritabanından veri okurken nesneyi ayağa kaldırması için zorunlu

        //Nesnemizi dış dünyadan güvenli bir şekilde yaratmak için Factory metot:
        public static MessageReaction Create(Guid chatRoomMessageId, Guid userId, string emoji)
        {
            if(string.IsNullOrWhiteSpace(emoji)) throw new ArgumentException("Emoji boş olamaz.");

            return new MessageReaction
            {
                Id = Guid.NewGuid(),
                ChatRoomMessageId = chatRoomMessageId,
                UserId = userId,
                Emoji = emoji,
                CreatedDate = DateTime.UtcNow
            };
        }

    }
}