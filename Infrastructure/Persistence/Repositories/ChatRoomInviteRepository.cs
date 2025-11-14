using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class ChatRoomInviteRepository : BaseRepository<ChatRoomInvite>, IChatRoomInviteRepository
    {
        public ChatRoomInviteRepository(ApplicationDbContext context) : base(context) { }

        public async Task<bool> HasPendingInviteAsync(
            Guid inviterUserId,
            Guid inviteeUserId,
            CancellationToken cancellationToken = default)
        {
            return await _context.ChatRoomInvites
                .AnyAsync(inv =>
                    inv.Status == InviteStatus.Pending &&
                    ((inv.InviterUserId == inviterUserId && inv.InviteeUserId == inviteeUserId) ||
                     (inv.InviterUserId == inviteeUserId && inv.InviteeUserId == inviterUserId)),
                    cancellationToken);
        }
    }
}
