using Application.Features.Branchs.DTOs;
using Application.Shared.Pagination;

namespace Application.Abstractions.QueryRepositories
{
    public interface IBranchQueryRepository
    {
        Task<BranchDto?> GetByIdAsync(Guid branchId, CancellationToken cancellationToken = default);

        Task<PaginatedResponse<BranchDto>> GetBranchesByBrandIdAsync(
            Guid brandId,
            PaginatedRequest pagination,
            CancellationToken cancellationToken = default);

        Task<PaginatedResponse<NearbyBranchDto>> GetNearbyBranchesAsync(
            decimal latitude,
            decimal longitude,
            int distanceInMeters,
            PaginatedRequest pagination, // Pagination parametresi
            CancellationToken cancellationToken = default);
    }
}
