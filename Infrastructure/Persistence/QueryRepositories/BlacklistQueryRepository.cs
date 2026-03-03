using Application.Abstractions.QueryRepositories;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.QueryRepositories
{
    public class BlacklistQueryRepository : IBlacklistQueryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public BlacklistQueryRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<bool> IsUserBannedAsync(Guid userId, Guid branchId, CancellationToken cancellationToken = default)
        {
            // O şubede, kullanıcının bitiş tarihi gelmemiş (aktif) bir banı var mı?
            return await _context.Set<Blacklist>()
                .AnyAsync(b => b.UserId == userId &&
                               b.BranchId == branchId &&
                               (b.FinishTime == null || b.FinishTime > DateTime.UtcNow),
                          cancellationToken);
        }
    }
}
