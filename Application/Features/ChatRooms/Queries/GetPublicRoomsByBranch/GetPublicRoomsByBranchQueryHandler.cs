using Application.Abstractions.QueryRepositories;
using Application.Features.ChatRooms.DTOs;
using Domain.Entities;
using Domain.SeedWork;
using MediatR;

namespace Application.Features.ChatRooms.Queries.GetPublicRoomsByBranch
{
    public class GetPublicRoomsByBranchQueryHandler : IRequestHandler<GetPublicRoomsByBranchQuery, List<ChatRoomDto>>
    {
        private readonly IChatRoomQueryRepository _chatRoomQueryRepository;
        private readonly IRepository<UserLocation> _userLocationRepository;

        public GetPublicRoomsByBranchQueryHandler(
            IChatRoomQueryRepository chatRoomQueryRepository,
            IRepository<UserLocation> userLocationRepository)
        {
            _chatRoomQueryRepository = chatRoomQueryRepository;
            _userLocationRepository = userLocationRepository;
        }

        public async Task<List<ChatRoomDto>> Handle(GetPublicRoomsByBranchQuery request, CancellationToken cancellationToken)
        {
            var currentLocation = await _userLocationRepository.GetAsync(
                ul => ul.UserId == request.UserId,
                cancellationToken);

            if (currentLocation == null)
                throw new UnauthorizedAccessException("Oda listelemek için önce bir şubeye check-in yapmalısınız.");

            return await _chatRoomQueryRepository.GetPublicRoomsByBranchIdAsync(
                currentLocation.BranchId,
                request.UserId,
                cancellationToken);
        }
    }
}
