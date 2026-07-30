using Application.Abstractions.QueryRepositories;
using Application.Features.ChatRooms.DTOs;
using MediatR;

namespace Application.Features.ChatRooms.Queries.GetPrivateInbox
{
    public class GetPrivateInboxQueryHandler : IRequestHandler<GetPrivateInboxQuery, List<ChatRoomDto>>
    {
        private readonly IChatRoomQueryRepository _chatRoomQueryRepository;

        public GetPrivateInboxQueryHandler(IChatRoomQueryRepository chatRoomQueryRepository)
        {
            _chatRoomQueryRepository = chatRoomQueryRepository;
        }

        public async Task<List<ChatRoomDto>> Handle(GetPrivateInboxQuery request, CancellationToken cancellationToken)
        {
            return await _chatRoomQueryRepository.GetPrivateInboxAsync(request.UserId, cancellationToken);
        }
    }
}
