using Application.Abstractions.QueryRepositories;
using Application.Features.Branchs.DTOs;
using MediatR;

namespace Application.Features.Branchs.Queries.GetNearbyBranches
{
    public class GetNearbyBranchesQueryHandler : IRequestHandler<GetNearbyBranchesQuery, List<NearbyBranchDto>>
    {
        private readonly IBranchQueryRepository _branchQueryRepository;

        public GetNearbyBranchesQueryHandler(IBranchQueryRepository branchQueryRepository)
        {
            _branchQueryRepository = branchQueryRepository;
        }

        public async Task<List<NearbyBranchDto>> Handle(GetNearbyBranchesQuery request, CancellationToken cancellationToken)
        {
            return await _branchQueryRepository.GetNearbyBranchesAsync(
                request.Latitude,
                request.Longitude,
                request.RadiusInMeters,
                cancellationToken
            );
        }
    }
}
