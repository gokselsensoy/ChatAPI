using Application.Abstractions.QueryRepositories;
using Application.Features.ChatRooms.DTOs;
using Application.Shared.Pagination;
using MediatR;

namespace Application.Features.ChatRooms.Queries.GetChatRoomMessages
{
    public class GetChatMessagesQueryHandler : IRequestHandler<GetChatMessagesQuery, PaginatedResponse<ChatRoomMessageDto>>
    {
        private readonly IChatRoomQueryRepository _queryRepository;
        public GetChatMessagesQueryHandler(IChatRoomQueryRepository queryRepository)
        { _queryRepository = queryRepository; }

        public async Task<PaginatedResponse<ChatRoomMessageDto>> Handle(GetChatMessagesQuery request, CancellationToken cancellationToken)
        {
            return await _queryRepository.GetMessagesForRoomAsync(request.RoomId, request, cancellationToken);
        }
    }
}
