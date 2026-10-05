using Domain.Entities;
using Domain.SeedWork;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Repositories
{
    // Standart Create, Update, Delete işlemleri zaten IRepository<BranchEvent> içinden gelecek
    public interface IBranchEventRepository : IRepository<BranchEvent> 
    {
        Task<BranchEvent?> GetByIdWithParticipantsAsync(Guid eventId, CancellationToken cancellationToken);
    }
}