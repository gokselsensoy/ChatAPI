using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class OrderRepository : BaseRepository<Order>, IOrderRepository
    {
        // BaseRepository zaten _context'e sahip, onu kullanıyoruz
        public OrderRepository(ApplicationDbContext context) : base(context)
        {
        }

        // IOrderRepository'deki spesifik metodu uyguluyoruz
        public async Task<Order?> GetByIdWithItemsAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
        }
    }
}
