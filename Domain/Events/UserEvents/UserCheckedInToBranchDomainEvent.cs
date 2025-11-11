using MediatR;

namespace Domain.Events.UserEvents
{
    public class UserCheckedInToBranchDomainEvent : INotification
    {
        public Guid UserId { get; }
        public Guid IdentityId { get; }
        public Guid? NewBranchId { get; }
        public Guid? OldBranchId { get; }

        public UserCheckedInToBranchDomainEvent(Guid userId, Guid identityId, Guid? newBranchId, Guid? oldBranchId)
        {
            UserId = userId;
            IdentityId=identityId;
            NewBranchId = newBranchId;
            OldBranchId = oldBranchId;
        }
    }
}
