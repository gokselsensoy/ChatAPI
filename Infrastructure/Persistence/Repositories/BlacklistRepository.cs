using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Persistence.Context;

namespace Infrastructure.Persistence.Repositories
{
    public class BlacklistRepository : BaseRepository<Blacklist>, IBlacklistRepository
    {
        public BlacklistRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
