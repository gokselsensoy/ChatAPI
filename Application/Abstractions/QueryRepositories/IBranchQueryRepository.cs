using Application.Features.Branchs.DTOs;
using Application.Shared.Pagination;
using System.Collections.Generic;

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

        /// <summary>
        /// Marka sahibi veya şube admini (BranchAdminMap) ise true döner.
        /// </summary>
        Task<bool> CanUserManageBranchAsync(Guid userId, Guid branchId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Şubede "Admin" rozeti gösterecek kullanıcı Id'leri (marka sahibi + atanmış şube adminleri).
        /// </summary>
        Task<HashSet<Guid>> GetBranchPrivilegedUserIdsAsync(Guid branchId, CancellationToken cancellationToken = default);

        Task<Guid?> GetBrandOwnerUserIdForBranchAsync(Guid branchId, CancellationToken cancellationToken = default);

        Task<List<BranchAdminListItemDto>> GetBranchAdminsAsync(Guid branchId, CancellationToken cancellationToken = default);
    }
}
