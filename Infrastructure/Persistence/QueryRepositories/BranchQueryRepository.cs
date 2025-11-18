using Application.Abstractions.QueryRepositories;
using Application.Features.Branchs.DTOs;
using Application.Shared.Pagination;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

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

        public async Task<List<NearbyBranchDto>> GetNearbyBranchesAsync(
            decimal latitude,
            decimal longitude,
            int distanceInMeters,
            CancellationToken cancellationToken = default)
        {
            var userLocation = new Point((double)longitude, (double)latitude) { SRID = 4326 };

            var query = _context.Branches
                    .AsNoTracking()
                    .Where(b => EF.Functions.IsWithinDistance(
                                    b.Address.Location,
                                    userLocation,
                                    distanceInMeters,
                                    true) // KRİTİK: true = useSpheroid (Metre cinsinden filtrele)
                    );
            var nearbyBranches = await query
                .OrderBy(b => b.Address.Location.Distance(userLocation) * 111195)
                .Select(b => new NearbyBranchDto
                {
                    Id = b.Id,
                    BrandId = b.BrandId,
                    Name = b.Name,
                    FileId = b.FileId,
                    BranchType = b.BranchType.ToString(),
                    Country = b.Address.Country,
                    City = b.Address.City,
                    District = b.Address.District,
                    Neighborhood = b.Address.Neighborhood,
                    Street = b.Address.Street,
                    BuildingNumber = b.Address.BuildingNumber,
                    ApartmentNumber = b.Address.ApartmentNumber,
                    ZipCode = b.Address.ZipCode,
                    Latitude = (decimal)b.Address.Location.Y,
                    Longitude = (decimal)b.Address.Location.X,
                    DistanceInMeters = b.Address.Location.Distance(userLocation) * 111195
                })
                .Take(50)
                .ToListAsync(cancellationToken);

            return nearbyBranches;
        }
    }
}
