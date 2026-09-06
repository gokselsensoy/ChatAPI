using Application.Abstractions.QueryRepositories;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Branchs.Queries.GetAvailableTags
{
    public class GetAvailableTagsQueryHandler : IRequestHandler<GetAvailableTagsQuery, List<string>>
    {
        private readonly IBranchQueryRepository _branchQueryRepository;

        public GetAvailableTagsQueryHandler(IBranchQueryRepository branchQueryRepository)
        {
            _branchQueryRepository = branchQueryRepository;
        }

        public async Task<List<string>> Handle(GetAvailableTagsQuery request, CancellationToken cancellationToken)
        {
            return await _branchQueryRepository.GetAvailableTagsAsync(
                request.Latitude,
                request.Longitude,
                request.DistanceInMeters,
                request.CurrentUserId,
                cancellationToken);
        }
    }
}
