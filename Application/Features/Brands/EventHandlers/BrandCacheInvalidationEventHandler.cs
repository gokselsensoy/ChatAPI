using Application.Features.Brands.Queries.GetAllBrands;
using Application.Features.Brands.Queries.GetBrandById;
using Application.Features.Brands.Queries.GetBrandByOwnerUserId;
using Application.Shared.EventHandlers;
using Domain.Events.BrandEvents;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Application.Features.Brands.EventHandlers
{
    public class BrandCacheInvalidationEventHandler : BaseCacheInvalidationEventHandler,
            INotificationHandler<BrandCreatedDomainEvent>,     // Yeni marka
            INotificationHandler<BrandOwnerUpdatedDomainEvent> // Marka güncellendi
    {
        private readonly ILogger<BrandCacheInvalidationEventHandler> _logger;

        public BrandCacheInvalidationEventHandler(
            IDistributedCache cache,
            ILogger<BrandCacheInvalidationEventHandler> logger)
            : base(cache, logger) // Base constructor'a cache ve logger'ı yolla
        {
            _logger = logger;
        }

        // 1. Yeni marka yaratıldı -> "Tüm Markalar" LİSTESİ geçersiz oldu
        public Task Handle(BrandCreatedDomainEvent notification, CancellationToken cancellationToken)
        {
            // CacheKey'i ilgili Query sınıfından alıyoruz
            var cacheKey = new GetAllBrandsQuery().CacheKey; // "brands:all"
            _logger.LogInformation("Event: {Event}. Cache temizleniyor: {CacheKey}", nameof(BrandCreatedDomainEvent), cacheKey);

            // Base class'taki yardımcı metodu çağır
            return ClearCacheAsync(cacheKey, cancellationToken);
        }

        // 2. Marka güncellendi -> LİSTE ve o markanın DETAYI geçersiz oldu
        public async Task Handle(BrandOwnerUpdatedDomainEvent notification, CancellationToken cancellationToken)
        {
            var listCacheKey = new GetAllBrandsQuery().CacheKey; // "brands:all"
            var detailCacheKey = new GetBrandByIdQuery { BrandId = notification.BrandId }.CacheKey; // "brand:{id}"
            var ownerCacheKey = new GetBrandByOwnerUserIdQuery { OwnerUserId = notification.OldOwnerUserId }.CacheKey;

            _logger.LogInformation("Event: {Event}. Cache temizleniyor: {Key1}, {Key2}, {Key3}", nameof(BrandOwnerUpdatedDomainEvent), listCacheKey, detailCacheKey, ownerCacheKey);

            await ClearCacheAsync(listCacheKey, cancellationToken);
            await ClearCacheAsync(detailCacheKey, cancellationToken);
        }
    }
}
