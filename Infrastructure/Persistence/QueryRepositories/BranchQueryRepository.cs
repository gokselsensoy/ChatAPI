using Application.Abstractions.QueryRepositories;
using Application.Features.Branchs.DTOs;
using Application.Shared.Pagination;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain;
using Domain.Entities;
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
    Guid? currentUserId,
    IReadOnlyList<string>? tags,
    PaginatedRequest pagination,
    CancellationToken cancellationToken = default)
        {
            // 1. Konum nesnesini oluştur
            var userLocation = new Point((double)longitude, (double)latitude) { SRID = 4326 };
            var random = new Random();

            var skipGeoFilter = false;
            if (currentUserId.HasValue)
            {
                var userId = currentUserId.Value;
                // Brand owner veya herhangi bir şubede atanmış admin ise mesafe filtresi bypass edilir.
                skipGeoFilter = await _context.Brands.AsNoTracking().AnyAsync(b => b.OwnerUserId == userId, cancellationToken)
                    || await _context.BranchAdminMaps.AsNoTracking().AnyAsync(m => m.UserId == userId, cancellationToken);
            }

            // 2. Temel Sorgu (Veritabanında filtreleme)
            var query = _context.Branches.AsNoTracking().AsQueryable();
            if (!skipGeoFilter)
            {
                query = query.Where(b => EF.Functions.IsWithinDistance(
                                    b.Address.Location,
                                    userLocation,
                                    distanceInMeters,
                                    true));
            }

            query = ApplyTagFilter(query, tags);

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
                item.DistanceInMeters = GeoConstants.DistanceInMeters(branchLocation, userLocation);
                item.CanCheckIn = item.DistanceInMeters <= GeoConstants.CheckInRadiusInMeters;

                // B. Doluluk Oranı (Şimdilik Mock Data)
                int fullness = random.Next(1, 10) * 10; // 10, 20... 100
                item.FullnessLevel = fullness;

                if (fullness <= 30) item.FullnessLabel = "Sakin";
                else if (fullness <= 70) item.FullnessLabel = "Hareketli";
                else item.FullnessLabel = "Çok Yoğun";
            }

            return new PaginatedResponse<NearbyBranchDto>(items, totalCount, pagination.PageNumber, pagination.PageSize);
        }

        public async Task<List<string>> GetAvailableTagsAsync(
            decimal latitude,
            decimal longitude,
            int distanceInMeters,
            Guid? currentUserId,
            CancellationToken cancellationToken = default)
        {
            var userLocation = new Point((double)longitude, (double)latitude) { SRID = 4326 };

            var skipGeoFilter = false;
            if (currentUserId.HasValue)
            {
                var userId = currentUserId.Value;
                skipGeoFilter = await _context.Brands.AsNoTracking().AnyAsync(b => b.OwnerUserId == userId, cancellationToken)
                    || await _context.BranchAdminMaps.AsNoTracking().AnyAsync(m => m.UserId == userId, cancellationToken);
            }

            var query = _context.Branches.AsNoTracking().AsQueryable();
            if (!skipGeoFilter)
            {
                query = query.Where(b => EF.Functions.IsWithinDistance(
                                    b.Address.Location,
                                    userLocation,
                                    distanceInMeters,
                                    true));
            }

            // Fetch the branches and select their Tags
            var tagsList = await query
                .Select(b => b.Tags)
                .ToListAsync(cancellationToken);

            // Flatten, clean, and distinct the tags
            var uniqueTags = tagsList
                .Where(t => t != null)
                .SelectMany(t => t)
                .Select(t => t.Value)
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(t => t)
                .ToList();

            return uniqueTags;
        }

        public async Task<bool> CanUserManageBranchAsync(Guid userId, Guid branchId, CancellationToken cancellationToken = default)
        {
            return await _context.Branches
                .AsNoTracking()
                .Where(b => b.Id == branchId)
                .AnyAsync(b => b.Brand!.OwnerUserId == userId
                    || _context.BranchAdminMaps.Any(m => m.BranchId == branchId && m.UserId == userId),
                    cancellationToken);
        }

        public async Task<HashSet<Guid>> GetBranchPrivilegedUserIdsAsync(Guid branchId, CancellationToken cancellationToken = default)
        {
            var ownerUserId = await _context.Branches
                .AsNoTracking()
                .Where(b => b.Id == branchId)
                .Select(b => (Guid?)b.Brand!.OwnerUserId)
                .FirstOrDefaultAsync(cancellationToken);

            var mapped = await _context.BranchAdminMaps
                .AsNoTracking()
                .Where(m => m.BranchId == branchId)
                .Select(m => m.UserId)
                .ToListAsync(cancellationToken);

            var set = new HashSet<Guid>(mapped);
            if (ownerUserId.HasValue)
                set.Add(ownerUserId.Value);

            return set;
        }

        public async Task<Guid?> GetBrandOwnerUserIdForBranchAsync(Guid branchId, CancellationToken cancellationToken = default)
        {
            return await _context.Branches
                .AsNoTracking()
                .Where(b => b.Id == branchId)
                .Select(b => (Guid?)b.Brand!.OwnerUserId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<BranchAdminListItemDto>> GetBranchAdminsAsync(Guid branchId, CancellationToken cancellationToken = default)
        {
            var ownerUserId = await GetBrandOwnerUserIdForBranchAsync(branchId, cancellationToken);

            var delegatedIds = await _context.BranchAdminMaps
                .AsNoTracking()
                .Where(m => m.BranchId == branchId)
                .Select(m => m.UserId)
                .ToListAsync(cancellationToken);

            var idSet = new HashSet<Guid>(delegatedIds);
            if (ownerUserId.HasValue)
                idSet.Add(ownerUserId.Value);

            if (idSet.Count == 0)
                return new List<BranchAdminListItemDto>();

            var delegatedSet = delegatedIds.ToHashSet();

            var users = await _context.Users
                .AsNoTracking()
                .Where(u => idSet.Contains(u.Id))
                .OrderBy(u => u.UserName)
                .ToListAsync(cancellationToken);

            var items = users.Select(u => new BranchAdminListItemDto
            {
                UserId = u.Id,
                UserName = u.UserName,
                FirstName = u.FirstName,
                LastName = u.LastName,
                IsBrandOwner = ownerUserId.HasValue && u.Id == ownerUserId.Value,
                IsDelegatedAdmin = delegatedSet.Contains(u.Id)
            }).ToList();

            return items
                .OrderByDescending(i => i.IsBrandOwner)
                .ThenBy(i => i.UserName)
                .ToList();
        }

        public async Task<bool> IsUserBrandOwnerAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Brands
                .AsNoTracking()
                .AnyAsync(b => b.OwnerUserId == userId, cancellationToken);
        }

        /// <summary>
        /// Tags JSON string kolonunda, seçilen etiketlerden en az birini içeren şubeleri bırakır.
        /// Null / boş liste / yalnızca boş string gelirse filtre uygulanmaz.
        /// </summary>
        private static IQueryable<Branch> ApplyTagFilter(IQueryable<Branch> query, IReadOnlyList<string>? tags)
        {
            var needles = tags?
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .SelectMany(t => t.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                .Where(t => t.Length > 0)
                .Select(t => "\"" + t.ToLowerInvariant() + "\"")
                .Distinct()
                .ToList();

            if (needles == null || needles.Count == 0)
                return query;

            return query.Where(b => needles.Any(n =>
                EF.Property<string>(b, nameof(Branch.Tags)).ToLower().Contains(n)));
        }
    }
}
