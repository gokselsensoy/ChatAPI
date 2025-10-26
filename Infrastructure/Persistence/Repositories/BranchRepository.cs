using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Persistence.Context;

namespace Infrastructure.Persistence.Repositories
{
    public class BranchRepository : BaseRepository<Branch>, IBranchRepository
    {
        public BranchRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
