using Application.Features.Users.Queries.GetMyProfile;
using Application.Shared.EventHandlers;
using Domain.Events.UserEvents;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Application.Features.Users.EventHandlers
{
    public class UserCacheInvalidationEventHandler : BaseCacheInvalidationEventHandler,
            INotificationHandler<UserProfileUpdatedDomainEvent>,   // Profil güncellendi
            INotificationHandler<UserCheckedInToBranchDomainEvent> // Şubeye check-in yaptı
    {
        private readonly ILogger<UserCacheInvalidationEventHandler> _logger;

        public UserCacheInvalidationEventHandler(
            IDistributedCache cache,
            ILogger<UserCacheInvalidationEventHandler> logger)
            : base(cache, logger)
        {
            _logger = logger;
        }

        // 1. Kullanıcı profilini (isim, resim vb.) güncellediğinde
        public Task Handle(UserProfileUpdatedDomainEvent notification, CancellationToken cancellationToken)
        {
            // GetMyProfileQuery'nin cache key'ini oluşturuyoruz
            var cacheKey = new GetMyProfileQuery { IdentityId = notification.IdentityId }.CacheKey; // "user:identity:{id}"

            _logger.LogInformation("Event: {Event}. Cache temizleniyor: {CacheKey}", nameof(UserProfileUpdatedDomainEvent), cacheKey);

            return ClearCacheAsync(cacheKey, cancellationToken);
        }

        // 2. Kullanıcı check-in yaptığında (BranchId'si değiştiğinde)
        public Task Handle(UserCheckedInToBranchDomainEvent notification, CancellationToken cancellationToken)
        {
            // GetMyProfileQuery'nin cache key'ini oluşturuyoruz
            // Düzeltme sayesinde artık 'notification.IdentityId'ye sahibiz.
            var cacheKey = new GetMyProfileQuery { IdentityId = notification.IdentityId }.CacheKey; // "user:identity:{id}"

            _logger.LogInformation("Event: {Event}. Cache temizleniyor: {CacheKey}", nameof(UserCheckedInToBranchDomainEvent), cacheKey);

            return ClearCacheAsync(cacheKey, cancellationToken);
        }
    }
}
