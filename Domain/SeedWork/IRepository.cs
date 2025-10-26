using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.SeedWork
{
    public interface IRepository<T> where T : IAggregateRoot
    {
        Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default);

        void Add(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}
