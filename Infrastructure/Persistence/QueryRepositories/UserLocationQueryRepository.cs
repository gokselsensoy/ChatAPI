using Application.Abstractions.QueryRepositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Persistence.QueryRepositories
{
    public class UserLocationQueryRepository : IUserLocationQueryRepository
    {
        private readonly ApplicationDbContext _context; // Kendi DbContext adın neyse o

        public UserLocationQueryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserLocation?> GetAsync(Expression<Func<UserLocation, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _context.UserLocations
                .AsNoTracking() // CQRS gereği sadece okuma yapıyoruz, takip etmiyoruz (Performans artar)
                                // Eğer UserLocation nesnesinde IsDeleted (Soft Delete) varsa onu da araya sıkıştırabilirsin:
                                // .Where(ul => !ul.IsDeleted) 
                .FirstOrDefaultAsync(predicate, cancellationToken);
        }

        public async Task<UserLocation?> GetActiveLocationByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.UserLocations
                .AsNoTracking() // EF Core bu nesneyi bellekte takip etmez, RAM dostudur
                                // Eğer Soft Delete (IsDeleted) mantığın varsa mutlaka sorguya eklemelisin
                .FirstOrDefaultAsync(ul => ul.UserId == userId && !ul.IsDeleted, cancellationToken);
        }

        public async Task<Guid?> GetActiveBranchIdByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            // Tüm nesneyi değil, sadece Guid (BranchId) dönerek SQL'de "SELECT BranchId FROM..." sorgusu atmasını sağlar.
            return await _context.UserLocations
                .AsNoTracking()
                .Where(ul => ul.UserId == userId && !ul.IsDeleted)
                .Select(ul => (Guid?)ul.BranchId)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
