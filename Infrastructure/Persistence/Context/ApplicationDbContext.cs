using Domain.Entities;
using Domain.SeedWork;
using Infrastructure.Identity.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OpenIddict.EntityFrameworkCore.Models;
using System.Reflection;

namespace Infrastructure.Persistence.Context
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>, IUnitOfWork
    {
        private readonly IPublisher _publisher;
        private IDbContextTransaction? _currentTransaction;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IPublisher publisher) : base(options)
        {
            _publisher = publisher;
        }

        #region UnitOfWork
        public bool HasActiveTransaction => _currentTransaction != null;

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction != null)
            {
                // Zaten bir transaction varsa (iç içe çağrılırsa) bir şey yapma
                return;
            }

            _currentTransaction = await Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_currentTransaction == null)
                    throw new InvalidOperationException("Aktif bir transaction bulunamadı.");

                await _currentTransaction.CommitAsync(cancellationToken);
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_currentTransaction == null)
                    throw new InvalidOperationException("Aktif bir transaction bulunamadı.");

                await _currentTransaction.RollbackAsync(cancellationToken);
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }
            }
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await PublishDomainEventsAsync(cancellationToken);
            return await base.SaveChangesAsync(cancellationToken);
        }
        #endregion

        public DbSet<Brand> Brands { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ChatRoom> ChatRooms { get; set; }
        public DbSet<ChatRoomInvite> ChatRoomInvites { get; set; }
        public DbSet<ChatRoomMessage> ChatRoomMessages { get; set; }
        public DbSet<ChatRoomUserMap> ChatRoomUserMaps { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<Announcement> Announcements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.UseOpenIddict<
                OpenIddictEntityFrameworkCoreApplication<Guid>,
                OpenIddictEntityFrameworkCoreAuthorization<Guid>,
                OpenIddictEntityFrameworkCoreScope<Guid>,
                OpenIddictEntityFrameworkCoreToken<Guid>,
                Guid>();

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        private async Task PublishDomainEventsAsync(CancellationToken cancellationToken)
        {
            // Takip edilen (tracked) Entity'leri bul
            var domainEntities = ChangeTracker
                .Entries<Entity>()
                .Select(entry => entry.Entity)
                .Where(entity => entity.DomainEvents.Any())
                .ToList();

            // Tüm event'leri topla
            var domainEvents = domainEntities
                .SelectMany(entity => entity.DomainEvents)
                .ToList();

            // Event'leri temizle ki tekrar fırlatılmasın
            domainEntities.ForEach(entity => entity.ClearDomainEvents());

            // MediatR aracılığıyla event'leri "yayınla" (publish)
            // Bu event'leri dinleyen Handler'lar (örn: OrderCreatedEmailHandler) çalışır
            foreach (var domainEvent in domainEvents)
            {
                await _publisher.Publish(domainEvent, cancellationToken);
            }
        }
    }
}
