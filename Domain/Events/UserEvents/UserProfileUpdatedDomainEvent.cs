using MediatR;

namespace Domain.Events.UserEvents
{
    public class UserProfileUpdatedDomainEvent : INotification
    {
        public Guid UserId { get; }
        public Guid IdentityId { get; }
        public string NewUserName { get; }

        public UserProfileUpdatedDomainEvent(Guid userId, Guid identityId, string newUserName)
        {
            UserId = userId;
            IdentityId = identityId;
            NewUserName = newUserName;
        }
    }
}
