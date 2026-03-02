using Application.Abstractions.QueryRepositories;
using Application.Features.Menus.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                .ToListAsync(cancellationToken); // <--- DEĞİŞİKLİK BURADA
        }
    }
}
