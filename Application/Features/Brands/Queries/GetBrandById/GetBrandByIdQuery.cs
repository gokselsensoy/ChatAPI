using Application.Abstractions.Messaging;
using Application.Features.Brands.DTOs;

namespace Application.Features.Brands.Queries.GetBrandById
{
    public class GetBrandByIdQuery : ICachableQuery<BrandDto>
    {
        public Guid BrandId { get; set; }

        public string CacheKey => $"brand:{BrandId}";
        public TimeSpan CacheDuration => TimeSpan.FromMinutes(10);
    }
}
