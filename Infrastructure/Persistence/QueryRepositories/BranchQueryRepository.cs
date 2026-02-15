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

        public async Task<PaginatedResponse<NearbyBranchDto>> GetNearbyBranchesAsync(
            decimal latitude,
            decimal longitude,
            int distanceInMeters,
            PaginatedRequest pagination, // Pagination parametresi
            CancellationToken cancellationToken = default)
        {
            // 1. Konum nesnesini oluştur
            var userLocation = new Point((double)longitude, (double)latitude) { SRID = 4326 };

            // Random ve Tag Havuzu
            var random = new Random();
            var availableTags = new List<string>
            {
                "Popüler", "Canlı Müzik", "Happy Hour", "Quiz Night",
                "Wi-Fi", "Maç Yayını", "Bahçe", "Rooftop", "Kokteyl Bar", "Vegan"
            };

            // 2. Temel Sorgu (Filtreleme)
            var query = _context.Branches
                    .AsNoTracking()
                    .Where(b => EF.Functions.IsWithinDistance(
                                    b.Address.Location,
                                    userLocation,
                                    distanceInMeters,
                                    true)
                    );

            // 3. Sıralama (Pagination için Skip/Take kullanmadan önce OrderBy ŞARTTIR)
            // Mesafeye göre en yakından uzağa sıralıyoruz
            query = query.OrderBy(b => b.Address.Location.Distance(userLocation));

            // 4. Toplam Kayıt Sayısı (Paging yapmadan önce alınmalı)
            var totalCount = await query.CountAsync(cancellationToken);

            // 5. Sayfalama ve Projection
            var items = await query
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ProjectTo<NearbyBranchDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            // 6. Hafızada (In-Memory) Hesaplamalar ve Mock Data
            foreach (var branch in items)
            {
                // Mesafe Hesabı
                var branchLocation = new Point((double)branch.Longitude, (double)branch.Latitude) { SRID = 4326 };
                branch.DistanceInMeters = branchLocation.Distance(userLocation) * 111195;

                // Mock Fullness (Her şube için rastgele olsun diye döngü içine aldım)
                int fullness = random.Next(1, 10) * 10;
                branch.FullnessLevel = fullness;

                if (fullness <= 30) branch.FullnessLabel = "Sakin";
                else if (fullness <= 70) branch.FullnessLabel = "Hareketli";
                else branch.FullnessLabel = "Çok Yoğun";

                // Mock Tags
                branch.Tags = availableTags
                    .OrderBy(x => random.Next())
                    .Take(random.Next(2, 4))
                    .ToList();
            }

            // 7. Paginated Response Döndür
            return new PaginatedResponse<NearbyBranchDto>(items, totalCount, pagination.PageNumber, pagination.PageSize);
        }
    }
}
