using Application.Features.Brands.DTOs;

namespace Application.Abstractions.QueryRepositories
{
    public interface IBrandQueryRepository
    {
        Task<BrandDto?> GetByIdAsync(Guid brandId, CancellationToken cancellationToken = default);
        Task<List<BrandDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<BrandDto?> GetByOwnerUserIdAsync(Guid ownerUserId, CancellationToken cancellationToken = default);
    }
}
