using MediatR;

namespace Domain.Events.UserEvents
{
    public class UserCreatedDomainEvent : INotification
    {
        public Guid UserId { get; }
        public Guid IdentityId { get; }
        public string UserName { get; }

        public UserCreatedDomainEvent(Guid userId, Guid identityId, string userName)
        {
            UserId = userId;
            IdentityId = identityId;
            UserName = userName;
        }
    }
}
