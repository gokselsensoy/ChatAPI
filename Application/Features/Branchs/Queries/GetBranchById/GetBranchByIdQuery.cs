using Application.Abstractions.Messaging;
using Application.Features.Branchs.DTOs;
using Application.Shared.Pagination;

namespace Application.Features.Branchs.Queries.GetBranchById
{
    public class GetBranchesByBrandIdQuery : PaginatedRequest, ICachableQuery<PaginatedResponse<BranchDto>>
    {
        public Guid BrandId { get; set; }

        // Cache anahtarı hem Marka ID'sini hem de sayfa bilgilerini içermeli
        public string CacheKey => $"brand:{BrandId}:branches:page:{PageNumber}:size:{PageSize}";
        public TimeSpan CacheDuration => TimeSpan.FromMinutes(10);
    }
}
