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
    }
}