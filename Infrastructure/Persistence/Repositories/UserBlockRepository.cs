using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class UserBlockRepository : BaseRepository<UserBlock>, IUserBlockRepository
    {
        public UserBlockRepository(ApplicationDbContext context) : base(context) { }

        public async Task<UserBlock?> GetBlockAsync(
            Guid blockerUserId,
            Guid blockedUserId,
            CancellationToken cancellationToken = default)
        {
            return await _context.UserBlocks
                .FirstOrDefaultAsync(b =>
                    !b.IsDeleted
                    && b.BlockerUserId == blockerUserId
                    && b.BlockedUserId == blockedUserId,
                    cancellationToken);
        }

        public async Task<bool> IsBlockedEitherWayAsync(
            Guid userId1,
            Guid userId2,
            CancellationToken cancellationToken = default)
        {
            return await _context.UserBlocks
                .AsNoTracking()
                .AnyAsync(b =>
                    !b.IsDeleted
                    && ((b.BlockerUserId == userId1 && b.BlockedUserId == userId2)
                        || (b.BlockerUserId == userId2 && b.BlockedUserId == userId1)),
                    cancellationToken);
        }

        public async Task<List<Guid>> GetBlockedUserIdsByBlockerAsync(
            Guid blockerUserId,
            CancellationToken cancellationToken = default)
        {
            return await _context.UserBlocks
                .AsNoTracking()
                .Where(b => !b.IsDeleted && b.BlockerUserId == blockerUserId)
                .Select(b => b.BlockedUserId)
                .ToListAsync(cancellationToken);
        }
    }
}
