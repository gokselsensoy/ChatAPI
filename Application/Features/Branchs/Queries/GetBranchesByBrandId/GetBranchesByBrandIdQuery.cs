using Application.Abstractions.Messaging;
using Application.Features.Branchs.DTOs;
using Application.Shared.Pagination;

namespace Application.Features.Branchs.Queries.GetBranchesByBrandId
{
    public class GetBranchesByBrandIdQuery : PaginatedRequest, ICachableQuery<PaginatedResponse<BranchDto>>
    {
        public Guid BrandId { get; set; }

        public string CacheKey => $"brand:{BrandId}:branches:page:{PageNumber}:size:{PageSize}";
        public TimeSpan CacheDuration => TimeSpan.FromMinutes(10);
    }
}
