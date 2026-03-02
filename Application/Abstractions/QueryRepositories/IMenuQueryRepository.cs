using Application.Features.Menus.DTOs;

namespace Application.Abstractions.QueryRepositories
{
    public interface IMenuQueryRepository
    {
        Task<List<MenuDto>> GetMenusWithItemsByBranchIdAsync(Guid branchId, CancellationToken cancellationToken = default);
    }
}
