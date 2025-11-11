using Application.Features.Branchs.Queries.GetBranchById;
using Application.Shared.EventHandlers;
using Domain.Events.BranchEvents;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Application.Features.Branchs.EventHandlers
{
    public class BranchCacheInvalidationEventHandler : BaseCacheInvalidationEventHandler,
        INotificationHandler<BranchCreatedDomainEvent>,    //
        INotificationHandler<BranchAddressUpdatedDomainEvent> //
    {
        private readonly ILogger<BranchCacheInvalidationEventHandler> _logger;

        public BranchCacheInvalidationEventHandler(
            IDistributedCache cache,
            ILogger<BranchCacheInvalidationEventHandler> logger)
            : base(cache, logger)
        {
            _logger = logger;
        }

        // 1. Yeni şube yaratıldı
        public Task Handle(BranchCreatedDomainEvent notification, CancellationToken cancellationToken)
        {
            // Bu mantık DOĞRU. 
            // "brand:{BrandId}:branches:page:1:size:10"
            // gibi dinamik anahtarları In-Memory cache (veya SCAN olmayan Redis) ile temizleyemeyiz.
            // Cache'in kendi kendine expire olmasını beklemek (veya Redis'te SCAN kullanmak) gerekir.
            _logger.LogWarning(
                "Event: {Event}. 'GetBranchesByBrandIdQuery' için cache temizlenemedi (dinamik paginasyonlu anahtar). " +
                "Cache'in {Duration}dk içinde kendi kendine expire olması beklenecek.",
                nameof(BranchCreatedDomainEvent), 10 // (CacheDuration'dan)
                );

            return Task.CompletedTask;
        }

        // 2. Şube güncellendi
        public Task Handle(BranchAddressUpdatedDomainEvent notification, CancellationToken cancellationToken)
        {
            // HATA DÜZELTME: Bu kod artık çalışacak çünkü GetBranchByIdQuery'yi düzelttik.
            var cacheKey = new GetBranchByIdQuery { BranchId = notification.BranchId }.CacheKey; // "branch:{id}"

            _logger.LogInformation("Event: {Event}. Cache temizleniyor: {CacheKey}", nameof(BranchAddressUpdatedDomainEvent), cacheKey);

            _logger.LogWarning("...Ayrıca 'GetBranchesByBrandIdQuery' paginasyonlu listesi de bayatlamış olabilir.");

            // Base class'taki ClearCacheAsync'i çağırıyoruz
            return ClearCacheAsync(cacheKey, cancellationToken);
        }
    }
}
