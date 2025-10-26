using Domain.Enums;
using Domain.SeedWork;

namespace Domain.Entities
{
    public class User : Entity, IAggregateRoot
    {
        public string UserName { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public UserType UserType { get; private set; }
        public string FileId { get; private set; }
        public Guid? BranchId { get; private set; }

        // Navigations
        public Branch Branch { get; private set; }
        public ICollection<ChatRoomMessages> SentMessages { get; private set; } = new List<ChatRoomMessages>();
        public ICollection<ChatRoomUserMap> ChatRoomMaps { get; private set; } = new List<ChatRoomUserMap>();
        public ICollection<ChatRoomInvite> SentInvites { get; private set; } = new List<ChatRoomInvite>();
        public ICollection<ChatRoomInvite> ReceivedInvites { get; private set; } = new List<ChatRoomInvite>();

        private User() { }
    }
}