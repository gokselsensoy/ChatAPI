using Domain.Entities;
using System.Linq.Expressions;

namespace Application.Abstractions.QueryRepositories
{
    public interface IUserLocationQueryRepository
    {
        Task<UserLocation?> GetAsync(Expression<Func<UserLocation, bool>> predicate, CancellationToken cancellationToken = default);
        // Kullanıcının o an aktif olarak bulunduğu lokasyonu (şubeyi) getirir
        Task<UserLocation?> GetActiveLocationByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

        // Sadece BranchId lazımsa, tüm nesneyi çekmek yerine çok daha performanslı olan bu metodu da ekleyebilirsin:
        Task<Guid?> GetActiveBranchIdByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
