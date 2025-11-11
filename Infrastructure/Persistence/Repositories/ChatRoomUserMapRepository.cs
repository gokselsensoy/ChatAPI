using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class ChatRoomUserMapRepository : BaseRepository<ChatRoomUserMap>, IChatRoomUserMapRepository
    {
        public ChatRoomUserMapRepository(ApplicationDbContext context) : base(context) { }

        public async Task<ChatRoomUserMap?> FindByRoomAndUserAsync(Guid chatRoomId, Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.ChatRoomUserMaps
                .FirstOrDefaultAsync(m => m.ChatRoomId == chatRoomId && m.UserId == userId, cancellationToken);
        }
    }
}
