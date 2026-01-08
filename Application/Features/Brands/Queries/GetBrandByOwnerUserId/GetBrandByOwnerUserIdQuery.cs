using Application.Abstractions.Messaging;
using Application.Features.Brands.DTOs;

namespace Application.Features.Brands.Queries.GetBrandByOwnerUserId
{
    public class GetBrandByOwnerUserIdQuery : ICachableQuery<BrandDto>
    {
        public Guid OwnerUserId { get; set; }

        public string CacheKey => $"brand:{OwnerUserId}";
        public TimeSpan CacheDuration => TimeSpan.FromMinutes(10);
    }
}
