using Application.Features.Menus.DTOs;

namespace Application.Abstractions.QueryRepositories
{
    public interface IMenuQueryRepository
    {
        Task<List<MenuDto>> GetMenusWithItemsByBranchIdAsync(Guid branchId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Marka sahibinin tüm şubeleri için menüleri (menü kalemleriyle) getirir; sonuç şube bazında gruplanır.
        /// </summary>
        Task<List<BranchMenusGroupDto>> GetMenusGroupedByBranchesForBrandOwnerAsync(Guid ownerUserId, CancellationToken cancellationToken = default);
    }
}
