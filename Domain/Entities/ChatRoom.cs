using Domain.Enums;
using Domain.Exceptions;
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
        public ICollection<ChatRoomMessage> Messages { get; private set; } = new List<ChatRoomMessage>();
        public ICollection<ChatRoomUserMap> ChatRoomUserMaps { get; private set; } = new List<ChatRoomUserMap>();
        public ICollection<ChatRoomInvite> ChatRoomInvites { get; private set; } = new List<ChatRoomInvite>();

        private ChatRoom() { }

        public static ChatRoom Create(
            string name,
            Guid branchId,
            RoomType roomType)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ChatRoomDomainException("Oda adı boş olamaz.");
            if (branchId == Guid.Empty)
                throw new ChatRoomDomainException("Oda bir şubeye bağlı olmalıdır.");

            var chatRoom = new ChatRoom
            {
                Id = Guid.NewGuid(),
                Name = name,
                BranchId = branchId,
                RoomType = roomType
            };

            // event fırlatılabilir: chatRoom.AddDomainEvent(new ChatRoomCreatedDomainEvent(...));

            return chatRoom;
        }

        public void UpdateDetails(string newName, RoomType newRoomType)
        {
            if (string.IsNullOrWhiteSpace(newName))
                throw new ChatRoomDomainException("Oda adı boş olamaz.");

            Name = newName;
            RoomType = newRoomType;
            // event fırlatılabilir
        }

        /// <summary>
        /// Bir kullanıcıyı odaya ekler (Join).
        /// </summary>
        public void AddUser(Guid userId, RoomType roomType, Guid userCurrentBranchId)
        {
            if (BranchId != userCurrentBranchId)
                throw new ChatRoomDomainException("Bu odaya katılmak için önce şubeye check-in yapmalısınız.");

            if (roomType != RoomType.Public)
                throw new ChatRoomDomainException("Bu oda gizlidir, sadece davetle girilebilir.");

            if (ChatRoomUserMaps.Any(m => m.UserId == userId))
                return;

            var map = ChatRoomUserMap.Create(Id, userId);
            ChatRoomUserMaps.Add(map);

            // event fırlatılabilir: AddDomainEvent(new UserJoinedRoomEvent(Id, userId));
        }

        /// <summary>
        /// Bir kullanıcıyı odadan çıkarır (Leave).
        /// </summary>
        public void RemoveUser(Guid userId)
        {
            var map = ChatRoomUserMaps.FirstOrDefault(m => m.UserId == userId);
            if (map == null)
                return;

            ChatRoomUserMaps.Remove(map);

            // event fırlatılabilir: AddDomainEvent(new UserLeftRoomEvent(Id, userId));
        }

        /// <summary>
        /// Odayı 'silindi' olarak işaretler.
        /// </summary>
        public void SetDeletedPrivateAndGroup()
        {
            if (RoomType == RoomType.Public)
                throw new ChatRoomDomainException("Public odalar silinemez.");

            IsDeleted = true;
            UpdatedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Odayı 'silindi' olarak işaretler.
        /// </summary>
        public void SetDeleted()
        {
            IsDeleted = true;
            UpdatedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Odaya yeni bir mesaj ekler.
        /// </summary>
        public ChatRoomMessage AddMessage(Guid senderUserId, string message)
        {
            if (!ChatRoomUserMaps.Any(m => m.UserId == senderUserId))
                throw new ChatRoomDomainException("Mesaj göndermek için önce odaya katılmalısınız.");

            var chatMessage = ChatRoomMessage.Create(Id, senderUserId, message);
            Messages.Add(chatMessage);

            // Not: Mesaj objesini geri döndürüyoruz ki Handler onu SignalR'a yollasın
            return chatMessage;
        }
    }
}