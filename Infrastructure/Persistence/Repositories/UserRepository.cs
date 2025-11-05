using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByIdentityIdAsync(Guid identityId, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.IdentityId == identityId, cancellationToken);
        }

        public async Task<bool> IsUserNameUniqueAsync(string userName, CancellationToken cancellationToken = default)
        {
            // Bu, AspNetUsers tablosunu kontrol etmeli
            return !await _context.Users
                .AnyAsync(u => u.UserName == userName, cancellationToken);
        }
    }
}
