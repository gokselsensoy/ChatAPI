using Domain.Entities;
using Domain.SeedWork;

namespace Domain.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByIdentityIdAsync(Guid identityId, CancellationToken cancellationToken = default);
        Task<bool> IsUserNameUniqueAsync(string userName, CancellationToken cancellationToken = default);
    }
}
