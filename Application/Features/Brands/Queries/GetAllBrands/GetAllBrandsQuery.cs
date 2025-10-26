using Application.Abstractions.Messaging;
using Application.Features.Brands.DTOs;

namespace Application.Features.Brands.Queries.GetAllBrands
{
    public class GetAllBrandsQuery : ICachableQuery<List<BrandDto>>
    {
        public string CacheKey => "brands:all";
        public TimeSpan CacheDuration => TimeSpan.FromMinutes(5);
    }
}
