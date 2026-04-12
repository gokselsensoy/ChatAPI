using Application.Abstractions.QueryRepositories;
using Application.Features.Menus.DTOs;
using MediatR;

namespace Application.Features.Menus.Queries.GetBranchAdminMenus
{
    public class GetBranchAdminMenusQueryHandler : IRequestHandler<GetBranchAdminMenusQuery, List<MenuDto>>
    {
        private readonly IMenuQueryRepository _menuQueryRepository;
        private readonly IBranchQueryRepository _branchQueryRepository;

        public GetBranchAdminMenusQueryHandler(
            IMenuQueryRepository menuQueryRepository,
            IBranchQueryRepository branchQueryRepository)
        {
            _menuQueryRepository = menuQueryRepository;
            _branchQueryRepository = branchQueryRepository;
        }

        public async Task<List<MenuDto>> Handle(GetBranchAdminMenusQuery request, CancellationToken cancellationToken)
        {
            if (!await _branchQueryRepository.CanUserManageBranchAsync(request.ActingUserId, request.BranchId, cancellationToken))
                throw new UnauthorizedAccessException("Bu şubenin menülerini yönetme yetkiniz yok.");

            return await _menuQueryRepository.GetMenusWithItemsByBranchIdAsync(request.BranchId, cancellationToken);
        }
    }
}
