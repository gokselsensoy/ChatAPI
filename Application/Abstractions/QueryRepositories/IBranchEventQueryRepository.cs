using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Abstractions.QueryRepositories
{
    public interface IBranchEventQueryRepository
    {
        Task<List<BranchEvent>> GetEventsByBranchIdAsync(Guid branchId, CancellationToken cancellationToken);
    }
}