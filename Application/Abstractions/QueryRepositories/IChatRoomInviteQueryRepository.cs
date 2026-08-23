using Application.Features.ChatRoomInvites.DTOs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Abstractions.QueryRepositories
{
    public interface IChatRoomInviteQueryRepository
    {
        Task<List<ChatRoomInviteDto>> GetPendingInvitesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
