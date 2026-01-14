using MediatR;

namespace Domain.Events.BranchEvents
{
    public class BranchUpdatedDomainEvent : INotification
    {
        public Guid BranchId { get; }
        public Guid BrandId { get; }
        public string BranchName { get; }

        public BranchUpdatedDomainEvent(Guid branchId, Guid brandId, string branchName)
        {
            BranchId = branchId;
            BrandId = brandId;
            BranchName = branchName;
        }
    }
}
