using Application.Abstractions.QueryRepositories;
using Application.Features.ChatRooms.DTOs;
using MediatR;

namespace Application.Features.ChatRooms.Queries.GetPublicRoomsByBranch
{
    public class GetPublicRoomsByBranchQueryHandler : IRequestHandler<GetPublicRoomsByBranchQuery, List<ChatRoomDto>>
    {
        private readonly IChatRoomQueryRepository _chatRoomQueryRepository;

        public GetPublicRoomsByBranchQueryHandler(IChatRoomQueryRepository chatRoomQueryRepository)
        {
            _chatRoomQueryRepository = chatRoomQueryRepository;
        }

        public async Task<List<ChatRoomDto>> Handle(GetPublicRoomsByBranchQuery request, CancellationToken cancellationToken)
        {
            return await _chatRoomQueryRepository.GetPublicRoomsByBranchIdAsync(request.BranchId, cancellationToken);
        }
    }
}
