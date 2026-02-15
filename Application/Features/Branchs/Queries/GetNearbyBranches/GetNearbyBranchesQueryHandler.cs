using Application.Abstractions.QueryRepositories;
using Application.Features.Branchs.DTOs;
using Application.Shared.Pagination;
using MediatR;

namespace Application.Features.Branchs.Queries.GetNearbyBranches
{
    public class GetNearbyBranchesQueryHandler : IRequestHandler<GetNearbyBranchesQuery, PaginatedResponse<NearbyBranchDto>>
    {
        private readonly IBranchQueryRepository _branchQueryRepository;

        public GetNearbyBranchesQueryHandler(IBranchQueryRepository branchQueryRepository)
        {
            _branchQueryRepository = branchQueryRepository;
        }

        public async Task<PaginatedResponse<NearbyBranchDto>> Handle(GetNearbyBranchesQuery request, CancellationToken cancellationToken)
        {
            // Request'in kendisi PaginatedRequest olduğu için direkt gönderiyoruz
            return await _branchQueryRepository.GetNearbyBranchesAsync(
                request.Latitude,
                request.Longitude,
                request.RadiusInMeters,
                request, // Pagination bilgileri burada
                cancellationToken
            );
        }
    }
}
