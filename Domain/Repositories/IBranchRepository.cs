using Domain.Entities;
using Domain.SeedWork;

namespace Domain.Repositories
{
    public interface IBranchRepository : IRepository<Branch>
    {
        Task<Branch?> GetByIdWithAdminMapsAsync(Guid branchId, CancellationToken cancellationToken = default);
    }
}
