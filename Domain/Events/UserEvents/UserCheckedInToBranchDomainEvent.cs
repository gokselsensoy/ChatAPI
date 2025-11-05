using MediatR;

namespace Domain.Events.UserEvents
{
    public class UserCheckedInToBranchDomainEvent : INotification
    {
        public Guid UserId { get; }
        public Guid? NewBranchId { get; }
        public Guid? OldBranchId { get; }

        public UserCheckedInToBranchDomainEvent(Guid userId, Guid? newBranchId, Guid? oldBranchId)
        {
            UserId = userId;
            NewBranchId = newBranchId;
            OldBranchId = oldBranchId;
        }
    }
}
