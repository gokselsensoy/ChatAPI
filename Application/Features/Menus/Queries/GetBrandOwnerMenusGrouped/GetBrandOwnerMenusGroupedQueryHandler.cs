using Application.Abstractions.QueryRepositories;
using Application.Features.Menus.DTOs;
using MediatR;

namespace Application.Features.Menus.Queries.GetBrandOwnerMenusGrouped
{
    public class GetBrandOwnerMenusGroupedQueryHandler : IRequestHandler<GetBrandOwnerMenusGroupedQuery, List<BranchMenusGroupDto>>
    {
        private readonly IMenuQueryRepository _menuQueryRepository;
        private readonly IBranchQueryRepository _branchQueryRepository;

        public GetBrandOwnerMenusGroupedQueryHandler(
            IMenuQueryRepository menuQueryRepository,
            IBranchQueryRepository branchQueryRepository)
        {
            _menuQueryRepository = menuQueryRepository;
            _branchQueryRepository = branchQueryRepository;
        }

        public async Task<List<BranchMenusGroupDto>> Handle(GetBrandOwnerMenusGroupedQuery request, CancellationToken cancellationToken)
        {
            if (!await _branchQueryRepository.IsUserBrandOwnerAsync(request.ActingUserId, cancellationToken))
                throw new UnauthorizedAccessException("Bu listeyi yalnızca marka sahibi görüntüleyebilir.");

            return await _menuQueryRepository.GetMenusGroupedByBranchesForBrandOwnerAsync(request.ActingUserId, cancellationToken);
        }
    }
}
