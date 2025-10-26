using Application.Abstractions.QueryRepositories;
using Application.Features.Branchs.DTOs;
using Application.Shared.Pagination;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.QueryRepositories
{
    public class BranchQueryRepository : IBranchQueryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public BranchQueryRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<BranchDto?> GetByIdAsync(Guid branchId, CancellationToken cancellationToken = default)
        {
            return await _context.Branches
                .AsNoTracking()
                .Where(b => b.Id == branchId)
                .ProjectTo<BranchDto>(_mapper.ConfigurationProvider, cancellationToken)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<PaginatedResponse<BranchDto>> GetBranchesByBrandIdAsync(
            Guid brandId,
            PaginatedRequest pagination,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Branches
                .AsNoTracking()
                .Where(b => b.BrandId == brandId)
                .OrderBy(b => b.Name)
                .ProjectTo<BranchDto>(_mapper.ConfigurationProvider, cancellationToken);

            // Sayfalama işlemini (Count, Skip, Take) veritabanında yap
            var count = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync(cancellationToken);

            return new PaginatedResponse<BranchDto>(items, count, pagination.PageNumber, pagination.PageSize);
        }
    }
}
