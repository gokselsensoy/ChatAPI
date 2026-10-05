using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Repositories
{
    // BaseRepository'den kalıtım aldığımız için ekleme/silme kodlarını tekrar yazmıyoruz
    public class BranchEventRepository : BaseRepository<BranchEvent>, IBranchEventRepository
    {
        public BranchEventRepository(ApplicationDbContext context) : base(context)
        {
        }

        // Katılımcı eklerken (Veya hatırlatıcı kurarken) lazım olacak özel metot
        public async Task<BranchEvent?> GetByIdWithParticipantsAsync(Guid eventId, CancellationToken cancellationToken)
        {
            return await _context.BranchEvents
                .Include(e => e.Participants)
                .FirstOrDefaultAsync(e => e.Id == eventId, cancellationToken);
        }
    }
}