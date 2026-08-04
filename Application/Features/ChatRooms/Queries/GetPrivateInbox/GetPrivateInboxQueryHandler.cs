using Application.Abstractions.QueryRepositories;
using Application.Abstractions.Services;
using Application.Features.ChatRooms.DTOs;
using MediatR;

namespace Application.Features.ChatRooms.Queries.GetPrivateInbox
{
    public class GetPrivateInboxQueryHandler : IRequestHandler<GetPrivateInboxQuery, List<ChatRoomDto>>
    {
        private readonly IChatRoomQueryRepository _chatRoomQueryRepository;
        private readonly IPresenceService _presenceService;

        public GetPrivateInboxQueryHandler(
            IChatRoomQueryRepository chatRoomQueryRepository,
            IPresenceService presenceService)
        {
            _chatRoomQueryRepository = chatRoomQueryRepository;
            _presenceService = presenceService;
        }

        public async Task<List<ChatRoomDto>> Handle(GetPrivateInboxQuery request, CancellationToken cancellationToken)
        {
            var rooms = await _chatRoomQueryRepository.GetPrivateInboxAsync(request.UserId, cancellationToken);

            var peerIds = rooms
                .Where(r => r.PeerUserId.HasValue)
                .Select(r => r.PeerUserId!.Value)
                .Distinct()
                .ToList();

            var onlineMap = _presenceService.GetOnlineStatus(peerIds);

            foreach (var room in rooms)
            {
                if (room.PeerUserId is Guid peerId)
                {
                    room.IsOnline = onlineMap.TryGetValue(peerId, out var online) && online;
                    room.OnlineMemberCount = room.IsOnline == true ? 1 : 0;
                }
            }

            return rooms;
        }
    }
}
