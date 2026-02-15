using Application.Abstractions.Messaging;
using Application.Features.Branchs.DTOs;
using Application.Shared.Pagination;

namespace Application.Features.Branchs.Queries.GetNearbyBranches
{
    public class GetNearbyBranchesQuery : PaginatedRequest, ICachableQuery<PaginatedResponse<NearbyBranchDto>>
    {
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public int RadiusInMeters { get; set; } = 5000; // Varsayılan 5km

        // Cache anahtarı konuma (yuvarlanmış) ve yarıçapa bağlı olmalı
        public string CacheKey => $"branches:nearby:lat:{Math.Round(Latitude, 4)}:lon:{Math.Round(Longitude, 4)}:r:{RadiusInMeters}";

        // Bu tür dinamik sorgular kısa süreli cache'lenir
        public TimeSpan CacheDuration => TimeSpan.FromMinutes(2);
    }
}
