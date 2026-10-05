using Domain.Entities;

namespace Application.Abstractions.QueryRepositories
{
    public interface IBranchEventQueryRepository
    {
        Task<List<BranchEvent>> GetEventsByBranchIdAsync(Guid branchId, CancellationToken cancellationToken);
    }
}