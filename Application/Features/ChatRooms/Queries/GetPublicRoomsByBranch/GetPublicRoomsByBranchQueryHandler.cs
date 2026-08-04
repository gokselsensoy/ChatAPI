using Application.Abstractions.QueryRepositories;
using Application.Abstractions.Services;
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
        private readonly IPresenceService _presenceService;

        public GetPublicRoomsByBranchQueryHandler(
            IChatRoomQueryRepository chatRoomQueryRepository,
            IRepository<UserLocation> userLocationRepository,
            IPresenceService presenceService)
        {
            _chatRoomQueryRepository = chatRoomQueryRepository;
            _userLocationRepository = userLocationRepository;
            _presenceService = presenceService;
        }

        public async Task<List<ChatRoomDto>> Handle(GetPublicRoomsByBranchQuery request, CancellationToken cancellationToken)
        {
            var currentLocation = await _userLocationRepository.GetAsync(
                ul => ul.UserId == request.UserId,
                cancellationToken);

            if (currentLocation == null)
                throw new UnauthorizedAccessException("Oda listelemek için önce bir şubeye check-in yapmalısınız.");

            var rooms = await _chatRoomQueryRepository.GetPublicRoomsByBranchIdAsync(
                currentLocation.BranchId,
                request.UserId,
                cancellationToken);

            await PresenceEnrichment.ApplyOnlineMemberCountsAsync(
                rooms,
                _chatRoomQueryRepository,
                _presenceService,
                cancellationToken);

            return rooms;
        }
    }
}
