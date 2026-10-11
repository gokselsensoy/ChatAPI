using MediatR;

namespace Domain.Events.BranchEvents
{
    public class BranchCreatedDomainEvent : INotification
    {
        public Guid BranchId { get; }
        public Guid BrandId { get; }
        public string BranchName { get; }

        public BranchCreatedDomainEvent(Guid branchId, Guid brandId, string branchName)
        {
            BranchId = branchId;
            BrandId = brandId;
            BranchName = branchName;
        }
    }
}
