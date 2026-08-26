using Application.Abstractions.QueryRepositories;
using Application.Features.Users.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using Infrastructure.Identity.Models;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Persistence.QueryRepositories
{
    public class UserQueryRepository : IUserQueryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public UserQueryRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<User?> GetAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _context.Set<User>()
                .AsNoTracking() // Sadece okuma yapılacaksa bu çok önemlidir
                .FirstOrDefaultAsync(predicate, cancellationToken);
        }

        public async Task<UserDto?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .ProjectTo<UserDto>(_mapper.ConfigurationProvider, cancellationToken)
                .FirstOrDefaultAsync(cancellationToken);

            if (user != null)
                user.BranchId = await GetActiveBranchIdAsync(user.Id, cancellationToken);

            return user;
        }

        public async Task<UserDto?> GetByIdentityIdAsync(Guid identityId, CancellationToken cancellationToken = default)
        {
            // 1. Bizim LOKAL profil tablomuzdan (Users) veriyi al
            // _context.Set<User>() kullanarak ambiguity'yi çöz
            var userProfile = await _context.Set<Domain.Entities.User>()
                .AsNoTracking()
                .Where(u => u.IdentityId == identityId)
                .ProjectTo<UserDto>(_mapper.ConfigurationProvider, cancellationToken)
                .FirstOrDefaultAsync(cancellationToken);

            if (userProfile == null) return null;

            // 2. AspNetUsers (ApplicationUser) tablosundan ek bilgileri al
            // _context.Set<ApplicationUser>() kullanarak DOĞRU tabloyu hedefle
            var identityInfo = await _context.Set<ApplicationUser>()
                .AsNoTracking()
                .Where(u => u.Id == identityId)
                .Select(u => new { u.Email, u.EmailConfirmed }) // Artık hata vermeyecek
                .FirstOrDefaultAsync(cancellationToken);

            if (identityInfo != null)
            {
                userProfile.Email = identityInfo.Email;
                // userProfile.EmailConfirmed = identityInfo.EmailConfirmed; // DTO'ya eklerseniz
            }

            userProfile.IsAnyBrandOwner = await _context.Brands
                .AsNoTracking()
                .AnyAsync(b => b.OwnerUserId == userProfile.Id, cancellationToken);

            userProfile.BranchId = await GetActiveBranchIdAsync(userProfile.Id, cancellationToken);

            if (userProfile.BranchId.HasValue)
            {
                var branchId = userProfile.BranchId.Value;
                userProfile.IsAdminAtCheckedInBranch = await _context.Branches
                    .AsNoTracking()
                    .Where(b => b.Id == branchId)
                    .AnyAsync(b => b.Brand!.OwnerUserId == userProfile.Id
                        || _context.BranchAdminMaps.Any(m => m.BranchId == branchId && m.UserId == userProfile.Id),
                        cancellationToken);
            }

            return userProfile;
        }

        public async Task<Dictionary<Guid, Guid?>> GetUserBranchMapAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
        {
            // 1. Önce bu kullanıcıların UserLocation tablosundaki kayıtlarını çekelim
            var locations = await _context.UserLocations
                .AsNoTracking()
                .Where(ul => userIds.Contains(ul.UserId) && !ul.IsDeleted)
                .Select(ul => new { ul.UserId, ul.BranchId })
                .ToListAsync(cancellationToken);

            // 2. Dictionary'yi oluştur. 
            // DİKKAT: Listede olmayan (hiç check-in yapmamış) kullanıcılar için 'null' dönmeliyiz.
            var result = new Dictionary<Guid, Guid?>();

            foreach (var userId in userIds)
            {
                var loc = locations.FirstOrDefault(x => x.UserId == userId);
                result[userId] = loc?.BranchId; // Kayıt varsa BranchId, yoksa null
            }

            return result;
        }

        public async Task<Dictionary<Guid, Guid>> GetIdentityIdsByUserIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
        {
            var list = userIds.Distinct().ToList();
            if (list.Count == 0)
                return new Dictionary<Guid, Guid>();

            var rows = await _context.Set<User>()
                .AsNoTracking()
                .Where(u => list.Contains(u.Id))
                .Select(u => new { u.Id, u.IdentityId })
                .ToListAsync(cancellationToken);

            return rows.ToDictionary(x => x.Id, x => x.IdentityId);
        }

        private async Task<Guid?> GetActiveBranchIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _context.UserLocations
                .AsNoTracking()
                .Where(ul => ul.UserId == userId && !ul.IsDeleted)
                .Select(ul => (Guid?)ul.BranchId)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
