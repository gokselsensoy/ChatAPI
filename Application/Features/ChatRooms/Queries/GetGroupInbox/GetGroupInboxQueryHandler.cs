using Application.Abstractions.QueryRepositories;
using Application.Abstractions.Services;
using Application.Features.ChatRooms.DTOs;
using MediatR;

namespace Application.Features.ChatRooms.Queries.GetGroupInbox
{
    public class GetGroupInboxQueryHandler : IRequestHandler<GetGroupInboxQuery, List<ChatRoomDto>>
    {
        private readonly IChatRoomQueryRepository _chatRoomQueryRepository;
        private readonly IPresenceService _presenceService;

        public GetGroupInboxQueryHandler(
            IChatRoomQueryRepository chatRoomQueryRepository,
            IPresenceService presenceService)
        {
            _chatRoomQueryRepository = chatRoomQueryRepository;
            _presenceService = presenceService;
        }

        public async Task<List<ChatRoomDto>> Handle(GetGroupInboxQuery request, CancellationToken cancellationToken)
        {
            var rooms = await _chatRoomQueryRepository.GetGroupInboxAsync(request.UserId, cancellationToken);
            await PresenceEnrichment.ApplyOnlineMemberCountsAsync(
                rooms,
                _chatRoomQueryRepository,
                _presenceService,
                cancellationToken);
            return rooms;
        }
    }
}
