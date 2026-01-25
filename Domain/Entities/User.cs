using Domain.Enums;
using Domain.Events.UserEvents;
using Domain.Exceptions;
using Domain.SeedWork;

namespace Domain.Entities
{
    public class User : Entity, IAggregateRoot
    {
        public Guid IdentityId { get; set; }
        public string UserName { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public UserType UserType { get; private set; }
        public string? FileId { get; private set; }
        public Guid? BranchId { get; private set; }

        // Navigations
        public Branch? Branch { get; private set; }
        public ICollection<ChatRoomMessage> SentMessages { get; private set; } = new List<ChatRoomMessage>();
        public ICollection<ChatRoomUserMap> ChatRoomMaps { get; private set; } = new List<ChatRoomUserMap>();
        public ICollection<ChatRoomInvite> SentInvites { get; private set; } = new List<ChatRoomInvite>();
        public ICollection<ChatRoomInvite> ReceivedInvites { get; private set; } = new List<ChatRoomInvite>();

        private User() { }

        // --- Fabrika Metodu ---
        // Bu, RegisterCommandHandler tarafından çağrılır
        public static User Create(Guid identityId, string userName, string firstName, string lastName, UserType userType)
        {
            if (identityId == Guid.Empty)
                throw new UserDomainException("IdentityId (AspNetUsers.Id) boş olamaz.");

            if (string.IsNullOrWhiteSpace(userName))
                throw new UserDomainException("Kullanıcı adı boş olamaz.");

            var user = new User
            {
                Id = Guid.NewGuid(),
                IdentityId = identityId,
                UserName = userName,
                FirstName = firstName,
                LastName = lastName,
                UserType = userType,
                BranchId = null
            };

            user.AddDomainEvent(new UserCreatedDomainEvent(user.Id, user.IdentityId, user.UserName));

            return user;
        }

        public void UpdateProfile(string userName, string firstName, string lastName, string? fileId)
        {
            UserName = userName;
            FirstName = firstName;
            LastName = lastName;
            FileId = fileId;

            AddDomainEvent(new UserProfileUpdatedDomainEvent(Id, IdentityId, userName));
        }

        public void CheckInToBranch(Guid? newBranchId)
        {
            if (BranchId == newBranchId)
                return;

            var oldBranchId = BranchId;
            BranchId = newBranchId;

            AddDomainEvent(new UserCheckedInToBranchDomainEvent(Id, IdentityId, newBranchId, oldBranchId));
        }
    }
}