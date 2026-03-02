using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class MenuRepository : BaseRepository<Menu>, IMenuRepository
    {
        public MenuRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Menu?> GetByIdWithItemsAsync(Guid menuId, CancellationToken cancellationToken = default)
        {
            return await _context.Menus
                .Include(m => m.MenuItems) // Ürünleri listeye doldur
                .FirstOrDefaultAsync(m => m.Id == menuId, cancellationToken);
        }
    }
}
