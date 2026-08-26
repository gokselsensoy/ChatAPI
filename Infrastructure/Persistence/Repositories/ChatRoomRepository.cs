using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class ChatRoomRepository : BaseRepository<ChatRoom>, IChatRoomRepository
    {
        public ChatRoomRepository(ApplicationDbContext context) : base(context) { }

        public async Task<ChatRoom?> GetByIdWithUsersAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.ChatRooms
                .Include(cr => cr.ChatRoomUserMaps)
                .FirstOrDefaultAsync(cr => cr.Id == id, cancellationToken);
        }

        public async Task<ChatRoom?> GetByIdWithMessagesAndUsersAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.ChatRooms
                .Include(cr => cr.ChatRoomUserMaps)
                .Include(cr => cr.Messages)
                .FirstOrDefaultAsync(cr => cr.Id == id, cancellationToken);
        }

        public async Task<ChatRoomMessage?> GetMessageByIdAsync(Guid messageId, CancellationToken cancellationToken = default)
        {
            return await _context.ChatRoomMessages
                .Include(m => m.SenderUser)
                .FirstOrDefaultAsync(m => m.Id == messageId && !m.IsDeleted, cancellationToken);
        }

        public async Task<List<ChatRoom>> GetRoomsByUserAndBranchAsync(Guid userId, Guid branchId, CancellationToken cancellationToken = default)
        {
            return await _context.ChatRooms
                .Include(r => r.ChatRoomUserMaps)
                .Where(r =>
                    r.BranchId == branchId &&
                    r.IsDeleted == false &&
                    r.ChatRoomUserMaps.Any(m => m.UserId == userId))
                .ToListAsync(cancellationToken);
        }

        public async Task MarkAsReadAsync(Guid roomId, Guid userId, CancellationToken cancellationToken = default)
        {
            var map = await _context.Set<ChatRoomUserMap>()
                .FirstOrDefaultAsync(m => m.ChatRoomId == roomId && m.UserId == userId, cancellationToken);

            if (map == null)
                return;

            map.MarkAsRead();
        }

        public async Task<ChatRoom?> FindDirectRoomBetweenUsersAsync(
            Guid userId1,
            Guid userId2,
            RoomType roomType,
            CancellationToken cancellationToken = default)
        {
            return await _context.ChatRooms
                .Include(cr => cr.ChatRoomUserMaps)
                .Where(cr =>
                    !cr.IsDeleted
                    && cr.RoomType == roomType
                    && cr.ChatRoomUserMaps.Count() == 2
                    && cr.ChatRoomUserMaps.Any(m => m.UserId == userId1)
                    && cr.ChatRoomUserMaps.Any(m => m.UserId == userId2))
                .OrderByDescending(cr => cr.CreatedDate)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<Guid>> GetSharedRoomPeerUserIdsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.ChatRooms
                .AsNoTracking()
                .Where(cr =>
                    !cr.IsDeleted
                    && (cr.RoomType == RoomType.Private || cr.RoomType == RoomType.Group)
                    && cr.ChatRoomUserMaps.Any(m => m.UserId == userId))
                .SelectMany(cr => cr.ChatRoomUserMaps
                    .Where(m => m.UserId != userId)
                    .Select(m => m.UserId))
                .Distinct()
                .ToListAsync(cancellationToken);
        }
    }
}
