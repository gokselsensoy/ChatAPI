using Application.Abstractions.Messaging;
using Application.Features.Branchs.DTOs;

namespace Application.Features.Branchs.Queries.GetBranchById
{
    public class GetBranchByIdQuery : ICachableQuery<BranchDto>
    {
        public Guid BranchId { get; set; }

        public string CacheKey => $"branch:{BranchId}";
        public TimeSpan CacheDuration => TimeSpan.FromMinutes(10);
    }
}
