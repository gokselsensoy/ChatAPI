using Application.Abstractions.QueryRepositories;
using Application.Features.Users.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Infrastructure.Identity.Models;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

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

        // ... (GetByIdAsync aynı) ...

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

            return userProfile;
        }
    }
}
