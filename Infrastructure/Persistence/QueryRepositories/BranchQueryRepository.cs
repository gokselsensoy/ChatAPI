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
            // 1. Veriyi Entity olarak çekiyoruz
            var branch = await _context.Branches
                .AsNoTracking()
                .Where(b => b.Id == branchId)
                .FirstOrDefaultAsync(cancellationToken);

            // 2. Eğer null geldiyse null dön
            if (branch == null) return null;

            // 3. Hafızada AutoMapper ile DTO'ya dönüştür
            return _mapper.Map<BranchDto>(branch);
        }

        public async Task<PaginatedResponse<BranchDto>> GetBranchesByBrandIdAsync(
            Guid brandId,
            PaginatedRequest pagination,
            CancellationToken cancellationToken = default)
        {
            // 1. Query'yi Branch Entity'si üzerinden kuruyoruz (ProjectTo SİLİNDİ)
            var query = _context.Branches
                .AsNoTracking()
                .Where(b => b.BrandId == brandId)
                .OrderBy(b => b.Name);

            // 2. Sayfalama ve Count işlemleri (Veritabanında yapılıyor)
            var count = await query.CountAsync(cancellationToken);

            // 3. Veriyi Branch listesi olarak çekiyoruz
            var branches = await query
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync(cancellationToken);

            // 4. Çekilen listeyi RAM'de DTO listesine çeviriyoruz
            var items = _mapper.Map<List<BranchDto>>(branches);

            return new PaginatedResponse<BranchDto>(items, count, pagination.PageNumber, pagination.PageSize);
        }

        public async Task<PaginatedResponse<NearbyBranchDto>> GetNearbyBranchesAsync(
    decimal latitude,
    decimal longitude,
    int distanceInMeters,
    PaginatedRequest pagination,
    CancellationToken cancellationToken = default)
        {
            // 1. Konum nesnesini oluştur
            var userLocation = new Point((double)longitude, (double)latitude) { SRID = 4326 };
            var random = new Random();

            // 2. Temel Sorgu (Veritabanında filtreleme)
            var query = _context.Branches
                .AsNoTracking()
                .Where(b => EF.Functions.IsWithinDistance(
                                b.Address.Location,
                                userLocation,
                                distanceInMeters,
                                true)
                );

            // 3. Mesafeye göre sırala
            query = query.OrderBy(b => b.Address.Location.Distance(userLocation));

            // 4. Toplam Kayıt Sayısı
            var totalCount = await query.CountAsync(cancellationToken);

            // 5. Veriyi Entity Olarak Çek (Çeviri Hatasını Önlemek İçin)
            var branches = await query
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync(cancellationToken);

            // 6. RAM'de DTO'ya Çevir 
            // (Tags, Country, Latitude vb. her şey burada otomatik eşleşir)
            var items = _mapper.Map<List<NearbyBranchDto>>(branches);

            // 7. AutoMapper'da Ignore Ettiğimiz Dinamik Alanları Doldur
            foreach (var item in items)
            {
                // A. Mesafe Hesaplama
                // (PostGIS'ten gelen SRID: 4326 derece cinsinden döner, metreye çevirmek için ~111195 ile çarpıyoruz)
                var branchLocation = new Point((double)item.Longitude, (double)item.Latitude) { SRID = 4326 };
                item.DistanceInMeters = branchLocation.Distance(userLocation) * 111195;

                // B. Doluluk Oranı (Şimdilik Mock Data)
                int fullness = random.Next(1, 10) * 10; // 10, 20... 100
                item.FullnessLevel = fullness;

                if (fullness <= 30) item.FullnessLabel = "Sakin";
                else if (fullness <= 70) item.FullnessLabel = "Hareketli";
                else item.FullnessLabel = "Çok Yoğun";
            }

            return new PaginatedResponse<NearbyBranchDto>(items, totalCount, pagination.PageNumber, pagination.PageSize);
        }
    }
}
