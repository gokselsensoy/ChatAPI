using Domain.Entities;
using Domain.SeedWork;

namespace Domain.Repositories
{
    public interface IBrandRepository : IRepository<Brand>
    {
        
    }

    public interface IMenuRepository : IRepository<Menu>
    {
        Task<Menu?> GetByIdWithItemsAsync(Guid menuId, CancellationToken cancellationToken = default);
    }
}
