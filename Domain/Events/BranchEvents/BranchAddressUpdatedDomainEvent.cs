using MediatR;

namespace Domain.Events.BranchEvents
{
    public class BranchAddressUpdatedDomainEvent : INotification
    {
        public Guid BranchId { get; }

        public BranchAddressUpdatedDomainEvent(Guid branchId)
        {
            BranchId = branchId;
        }
    }
}
