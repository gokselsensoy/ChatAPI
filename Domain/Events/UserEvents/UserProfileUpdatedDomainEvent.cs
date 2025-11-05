using MediatR;

namespace Domain.Events.UserEvents
{
    public class UserProfileUpdatedDomainEvent : INotification
    {
        public Guid UserId { get; }
        public Guid IdentityId { get; }

        public UserProfileUpdatedDomainEvent(Guid userId, Guid identityId)
        {
            UserId = userId;
            IdentityId = identityId;
        }
    }
}
