using Application.Abstractions.QueryRepositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.QueryRepositories
{
    public class BranchEventQueryRepository : IBranchEventQueryRepository
    {
        private readonly ApplicationDbContext _context;

        public BranchEventQueryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<BranchEvent>> GetEventsByBranchIdAsync(Guid branchId, CancellationToken cancellationToken)
        {
            // AsNoTracking() ile okuma performansını artırıyoruz çünkü veri güncellenmeyecek.
            return await _context.BranchEvents
                .Include(e => e.Participants) // Katılımcı verilerini eklemezsek buton durumlarını hesaplayamayız
                .AsNoTracking()
                .Where(e => e.BranchId == branchId)
                .OrderBy(e => e.StartDate)
                .ToListAsync(cancellationToken);
        }
    }
}