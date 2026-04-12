using Application.Abstractions.QueryRepositories;
using Application.Features.Menus.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.QueryRepositories
{
    public class MenuQueryRepository : IMenuQueryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public MenuQueryRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<MenuDto>> GetMenusWithItemsByBranchIdAsync(Guid branchId, CancellationToken cancellationToken = default)
        {
            return await _context.Menus
                .AsNoTracking()
                .Where(m => m.BranchId == branchId)
                .ProjectTo<MenuDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<BranchMenusGroupDto>> GetMenusGroupedByBranchesForBrandOwnerAsync(Guid ownerUserId, CancellationToken cancellationToken = default)
        {
            var branches = await _context.Set<Branch>()
                .AsNoTracking()
                .Include(b => b.Brand)
                .Where(b => b.Brand != null && b.Brand.OwnerUserId == ownerUserId)
                .OrderBy(b => b.Brand!.Name)
                .ThenBy(b => b.Name)
                .ToListAsync(cancellationToken);

            if (branches.Count == 0)
                return new List<BranchMenusGroupDto>();

            var branchIds = branches.Select(b => b.Id).ToList();

            var menus = await _context.Menus
                .AsNoTracking()
                .Where(m => branchIds.Contains(m.BranchId))
                .ProjectTo<MenuDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return branches.Select(b =>
            {
                var dto = _mapper.Map<BranchMenusGroupDto>(b);
                dto.Menus = menus.Where(m => m.BranchId == b.Id).ToList();
                return dto;
            }).ToList();
        }
    }
}
