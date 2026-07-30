using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class UserDeviceTokenRepository : BaseRepository<UserDeviceToken>, IUserDeviceTokenRepository
    {
        public UserDeviceTokenRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<UserDeviceToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            return await _context.Set<UserDeviceToken>()
                .FirstOrDefaultAsync(t => t.Token == token, cancellationToken);
        }

        public async Task<List<string>> GetActiveTokensByUserIdsAsync(
            IEnumerable<Guid> userIds,
            CancellationToken cancellationToken = default)
        {
            var idList = userIds.Distinct().ToList();
            if (idList.Count == 0)
                return new List<string>();

            return await _context.Set<UserDeviceToken>()
                .AsNoTracking()
                .Where(t => idList.Contains(t.UserId) && t.IsActive && !t.IsDeleted)
                .Select(t => t.Token)
                .Distinct()
                .ToListAsync(cancellationToken);
        }
    }
}
