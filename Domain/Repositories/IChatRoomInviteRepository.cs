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

        Task<ChatRoomInvite?> GetByIdWithRoomAsync(Guid id, CancellationToken cancellationToken = default);

        Task<List<ChatRoomInvite>> GetPendingIncomingWithDetailsAsync(
            Guid inviteeUserId,
            CancellationToken cancellationToken = default);
    }
}
