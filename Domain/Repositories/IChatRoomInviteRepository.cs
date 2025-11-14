using Domain.Entities;
using Domain.SeedWork;

namespace Domain.Repositories
{
    public interface IChatRoomInviteRepository : IRepository<ChatRoomInvite>
    {
        Task<bool> HasPendingInviteAsync(
            Guid inviterUserId,
            Guid inviteeUserId,
            CancellationToken cancellationToken = default);
    }
}
