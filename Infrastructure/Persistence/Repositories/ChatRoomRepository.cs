using Domain.Entities;
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
                .Include(cr => cr.ChatRoomUserMaps) // Child'ları yüklüyoruz
                .FirstOrDefaultAsync(cr => cr.Id == id, cancellationToken);
        }

        // YENİ METOT IMPLEMENTASYONU
        public async Task<ChatRoom?> GetByIdWithMessagesAndUsersAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.ChatRooms
                .Include(cr => cr.ChatRoomUserMaps) // Gerekli
                .Include(cr => cr.Messages) // Gerekli
                .FirstOrDefaultAsync(cr => cr.Id == id, cancellationToken);
        }

        public async Task<List<ChatRoom>> GetRoomsByUserAndBranchAsync(Guid userId, Guid branchId, CancellationToken cancellationToken = default)
        {
            return await _context.ChatRooms
                .Include(r => r.ChatRoomUserMaps) // Kullanıcı listesini dahil et ki silebilelim
                .Where(r =>
                    r.BranchId == branchId &&
                    r.IsDeleted == false && // Silinmemiş odalar
                    r.ChatRoomUserMaps.Any(m => m.UserId == userId)) // Kullanıcının içinde olduğu odalar
                .ToListAsync(cancellationToken);
        }
    }
}
