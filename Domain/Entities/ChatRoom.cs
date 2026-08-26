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

            return chatRoom;
        }

        public void UpdateDetails(string newName, RoomType newRoomType)
        {
            if (string.IsNullOrWhiteSpace(newName))
                throw new ChatRoomDomainException("Oda adı boş olamaz.");

            Name = newName;
            RoomType = newRoomType;
        }

        public void JoinPublicRoom(Guid userId, Guid userCurrentBranchId)
        {
            if (BranchId != userCurrentBranchId)
                throw new ChatRoomDomainException("Bu odaya katılmak için önce şubeye check-in yapmalısınız.");

            if (RoomType != RoomType.Public)
                throw new ChatRoomDomainException("Bu oda gizlidir, sadece davetle girilebilir.");

            AddUserInternal(userId);
        }

        public void JoinViaInvite(Guid userId, Guid userCurrentBranchId)
        {
            if (BranchId != userCurrentBranchId)
                throw new ChatRoomDomainException("Bu odaya katılmak için önce şubeye check-in yapmalısınız.");

            AddUserInternal(userId);
        }

        private void AddUserInternal(Guid userId)
        {
            if (ChatRoomUserMaps.Any(m => m.UserId == userId))
                return;

            var map = ChatRoomUserMap.Create(Id, userId);
            ChatRoomUserMaps.Add(map);
        }

        public void RemoveUser(Guid userId)
        {
            var map = ChatRoomUserMaps.FirstOrDefault(m => m.UserId == userId);
            if (map == null)
                return;

            ChatRoomUserMaps.Remove(map);
        }

        public void SetDeletedPrivateAndGroup()
        {
            if (RoomType == RoomType.Public)
                throw new ChatRoomDomainException("Public odalar silinemez.");

            IsDeleted = true;
            UpdatedDate = DateTime.UtcNow;
        }

        public void SetDeleted()
        {
            IsDeleted = true;
            UpdatedDate = DateTime.UtcNow;
        }

        public ChatRoomMessage AddMessage(Guid senderUserId, string message, ChatRoomMessage? replyToMessage = null)
        {
            if (!ChatRoomUserMaps.Any(m => m.UserId == senderUserId))
                throw new ChatRoomDomainException("Mesaj göndermek için önce odaya katılmalısınız.");

            Guid? replyToMessageId = null;
            if (replyToMessage != null)
            {
                if (replyToMessage.ChatRoomId != Id || replyToMessage.IsDeleted)
                    throw new ChatRoomDomainException("Yanıtlanan mesaj bu odada bulunamadı.");

                replyToMessageId = replyToMessage.Id;
            }

            var chatMessage = ChatRoomMessage.Create(Id, senderUserId, message, replyToMessageId);
            Messages.Add(chatMessage);
            return chatMessage;
        }

        /// <summary>Private veya Group — üyelik zorunlu.</summary>
        public bool IsMemberOnlyRoom =>
            RoomType is RoomType.Private or RoomType.Group;

        /// <summary>Public + Group check-in ister; Private geo'suz.</summary>
        public bool RequiresCheckInForMessaging =>
            RoomType is RoomType.Public or RoomType.Group;
    }
}
