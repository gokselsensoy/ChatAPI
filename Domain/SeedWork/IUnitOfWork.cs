namespace Domain.SeedWork
{
    // IUnitOfWork arayüzü
    public interface IUnitOfWork : IDisposable
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Yeni bir veritabanı işlemi (transaction) başlatır.
        /// </summary>
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Mevcut işlemi onaylar (commit).
        /// </summary>
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Mevcut işlemi geri alır (rollback).
        /// </summary>
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Aktif bir transaction olup olmadığını döner.
        /// </summary>
        bool HasActiveTransaction { get; }
    }
}
