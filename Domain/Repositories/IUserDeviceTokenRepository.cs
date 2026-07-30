using Domain.Entities;
using Domain.SeedWork;

namespace Domain.Repositories
{
    public interface IUserDeviceTokenRepository : IRepository<UserDeviceToken>
    {
        Task<UserDeviceToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
        Task<List<string>> GetActiveTokensByUserIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);
    }
}
