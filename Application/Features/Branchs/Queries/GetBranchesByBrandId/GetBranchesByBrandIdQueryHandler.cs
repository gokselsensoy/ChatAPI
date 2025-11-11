using Application.Abstractions.QueryRepositories;
using Application.Exceptions;
using Application.Features.Branchs.DTOs;
using Application.Shared.Pagination;
using Domain.Entities;
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
            // HATA DÜZELTME: GetByIdAsync -> GetBranchesByBrandIdAsync oldu
            // 'request'in kendisi PaginatedRequest'ten kalıtım aldığı için
            // sayfalama parametrelerini de (PageNumber, PageSize) içerir.
            return await _branchQueryRepository.GetBranchesByBrandIdAsync(
                request.BrandId,
                request, // Sayfalama parametreleri için request'in kendisini yolluyoruz
                cancellationToken
            );
        }
    }
}
