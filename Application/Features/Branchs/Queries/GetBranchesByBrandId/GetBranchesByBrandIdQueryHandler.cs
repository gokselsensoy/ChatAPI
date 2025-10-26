using Application.Abstractions.QueryRepositories;
using Application.Features.Branchs.DTOs;
using Application.Shared.Pagination;
using MediatR;

namespace Application.Features.Branchs.Queries.GetBranchesByBrandId
{
    public class GetBranchesByBrandIdQueryHandler : IRequestHandler<GetBranchesByBrandIdQuery, PaginatedResponse<BranchDto>>
    {
        private readonly IBranchQueryRepository _branchQueryRepository;

        public GetBranchesByBrandIdQueryHandler(IBranchQueryRepository branchQueryRepository)
        {
            _branchQueryRepository = branchQueryRepository;
        }

        public async Task<PaginatedResponse<BranchDto>> Handle(GetBranchesByBrandIdQuery request, CancellationToken cancellationToken)
        {
            // request'in kendisi PaginatedRequest'ten kalıtım aldığı için
            // sayfalama bilgilerini (PageNumber, PageSize) içerir.
            return await _branchQueryRepository.GetBranchesByBrandIdAsync(
                request.BrandId,
                request, // Sayfalama parametreleri için request'in kendisini yolluyoruz
                cancellationToken
            );
        }
    }
}
