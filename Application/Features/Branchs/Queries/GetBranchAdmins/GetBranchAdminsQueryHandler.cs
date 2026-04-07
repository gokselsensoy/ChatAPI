using Application.Abstractions.QueryRepositories;
using Application.Features.Branchs.DTOs;
using MediatR;

namespace Application.Features.Branchs.Queries.GetBranchAdmins
{
    public class GetBranchAdminsQueryHandler : IRequestHandler<GetBranchAdminsQuery, List<BranchAdminListItemDto>>
    {
        private readonly IBranchQueryRepository _branchQueryRepository;

        public GetBranchAdminsQueryHandler(IBranchQueryRepository branchQueryRepository)
        {
            _branchQueryRepository = branchQueryRepository;
        }

        public async Task<List<BranchAdminListItemDto>> Handle(GetBranchAdminsQuery request, CancellationToken cancellationToken)
        {
            if (!await _branchQueryRepository.CanUserManageBranchAsync(request.ActingUserId, request.BranchId, cancellationToken))
                throw new UnauthorizedAccessException("Bu şubenin yönetici listesini görüntüleme yetkiniz yok.");

            return await _branchQueryRepository.GetBranchAdminsAsync(request.BranchId, cancellationToken);
        }
    }
}
