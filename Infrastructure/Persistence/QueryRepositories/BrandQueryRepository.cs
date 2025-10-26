using Application.Abstractions.QueryRepositories;
using Application.Features.Brands.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.QueryRepositories
{
    public class BrandQueryRepository : IBrandQueryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public BrandQueryRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<BrandDto?> GetByIdAsync(Guid brandId, CancellationToken cancellationToken = default)
        {
            // ProjectTo<BrandDto> kullanarak veritabanından sadece
            // DTO'nun ihtiyaç duyduğu kolonları çekeriz (optimize sorgu).
            return await _context.Brands
                .AsNoTracking() // Sadece okuma yapıyoruz, tracking gerekmez
                .Where(b => b.Id == brandId)
                .ProjectTo<BrandDto>(_mapper.ConfigurationProvider, cancellationToken)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<BrandDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Brands
                .AsNoTracking()
                .OrderBy(b => b.Name)
                .ProjectTo<BrandDto>(_mapper.ConfigurationProvider, cancellationToken)
                .ToListAsync(cancellationToken);
        }
    }
}
