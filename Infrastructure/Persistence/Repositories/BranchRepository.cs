using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class BranchRepository : BaseRepository<Branch>, IBranchRepository
    {
        public BranchRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Branch?> GetByIdWithAdminMapsAsync(Guid branchId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Branch>()
                .Include(b => b.BranchAdminMaps)
                .FirstOrDefaultAsync(b => b.Id == branchId, cancellationToken);
        }
    }
}
