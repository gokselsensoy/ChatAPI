using Application.Abstractions.QueryRepositories;
using Application.Features.ChatRoomInvites.DTOs;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.ChatRoomInvites.Queries.GetMyPendingInvites
{
    public class GetMyPendingInvitesQueryHandler : IRequestHandler<GetMyPendingInvitesQuery, List<ChatRoomInviteDto>>
    {
        private readonly IChatRoomInviteQueryRepository _queryRepository;

        public GetMyPendingInvitesQueryHandler(IChatRoomInviteQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<List<ChatRoomInviteDto>> Handle(GetMyPendingInvitesQuery request, CancellationToken cancellationToken)
        {
            return await _queryRepository.GetPendingInvitesByUserIdAsync(request.UserId, cancellationToken);
        }
    }
}
