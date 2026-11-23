using Domain.Entities;
using Domain.SeedWork;

namespace Domain.Repositories
{
    public interface IUserBlockRepository : IRepository<UserBlock>
    {
        Task<UserBlock?> GetBlockAsync(
            Guid blockerUserId,
            Guid blockedUserId,
            CancellationToken cancellationToken = default);

        Task<bool> IsBlockedEitherWayAsync(
            Guid userId1,
            Guid userId2,
            CancellationToken cancellationToken = default);

        Task<List<Guid>> GetBlockedUserIdsByBlockerAsync(
            Guid blockerUserId,
            CancellationToken cancellationToken = default);
    }
}
