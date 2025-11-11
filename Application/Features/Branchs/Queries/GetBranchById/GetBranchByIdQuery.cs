using Application.Abstractions.Messaging;
using Application.Features.Branchs.DTOs;
using Application.Shared.Pagination;
using MediatR;

namespace Application.Features.Branchs.Queries.GetBranchById
{
    public class GetBranchByIdQuery : ICachableQuery<BranchDto>
    {
        public Guid BranchId { get; set; }

        // CacheKey ve CacheDuration eklendi (EventHandler'ın buna ihtiyacı var)
        public string CacheKey => $"branch:{BranchId}";
        public TimeSpan CacheDuration => TimeSpan.FromMinutes(10);
    }
}
