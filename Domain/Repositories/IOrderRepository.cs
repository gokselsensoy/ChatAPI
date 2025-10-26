using Domain.Entities;
using Domain.SeedWork;

namespace Domain.Repositories
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<Order?> GetByIdWithItemsAsync(Guid orderId, CancellationToken cancellationToken = default);
    }
}
