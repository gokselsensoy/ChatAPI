using Application.Features.Brands.Queries.GetAllBrands;
using Application.Features.Brands.Queries.GetBrandById;
using Application.Features.Users.Queries.GetMyProfile;
using Domain.Events.BranchEvents;
using Domain.Events.BrandEvents;
using Domain.Events.UserEvents;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Application.Shared.EventHandlers
{
    /// <summary>
    /// Bu merkezi handler, Domain Event'lerini dinler ve 
    /// CachingPipelineBehaviour tarafından oluşturulan 
    /// ilgili cache anahtarlarını temizler.
    /// ProjectBase'deki yapıya benzer.
    /// </summary>
    public class CacheInvalidationEventHandler :
        // Brand Events
        INotificationHandler<BrandCreatedDomainEvent>,
        INotificationHandler<BrandOwnerUpdatedDomainEvent>,
        // (Eğer Brand detayları güncellenirse buraya eklenmeli)

        // Branch Events
        INotificationHandler<BranchCreatedDomainEvent>,
        INotificationHandler<BranchAddressUpdatedDomainEvent>,

        // User Events
        INotificationHandler<UserProfileUpdatedDomainEvent>,
        INotificationHandler<UserCheckedInToBranchDomainEvent>
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<CacheInvalidationEventHandler> _logger;

        public CacheInvalidationEventHandler(
            IDistributedCache cache,
            ILogger<CacheInvalidationEventHandler> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        // --- Brand Handlers ---

        // Yeni marka yaratıldı -> "Tüm Markalar" listesi geçersiz oldu
        public Task Handle(BrandCreatedDomainEvent notification, CancellationToken cancellationToken)
        {
            var cacheKey = new GetAllBrandsQuery().CacheKey; // "brands:all"
            _logger.LogInformation("Event: {Event}. Cache temizleniyor: {CacheKey}", nameof(BrandCreatedDomainEvent), cacheKey);
            return ClearCacheAsync(cacheKey, cancellationToken);
        }

        // Marka güncellendi -> "Tüm Markalar" LİSTESİ ve o markanın DETAYI geçersiz oldu
        public async Task Handle(BrandOwnerUpdatedDomainEvent notification, CancellationToken cancellationToken)
        {
            var listCacheKey = new GetAllBrandsQuery().CacheKey; // "brands:all"
            var detailCacheKey = new GetBrandByIdQuery { BrandId = notification.BrandId }.CacheKey; // "brand:{id}"

            _logger.LogInformation("Event: {Event}. Cache temizleniyor: {Key1}, {Key2}", nameof(BrandOwnerUpdatedDomainEvent), listCacheKey, detailCacheKey);

            await ClearCacheAsync(listCacheKey, cancellationToken);
            await ClearCacheAsync(detailCacheKey, cancellationToken);
        }

        // --- Branch Handlers ---

        // Yeni şube yaratıldı -> O markaya ait "Şube Listesi" geçersiz oldu
        public Task Handle(BranchCreatedDomainEvent notification, CancellationToken cancellationToken)
        {
            // Bu event, hangi sayfalamanın cache'ini temizleyeceğini bilemez
            // (Page=1, Page=2 vb.)
            // Bu durumda ya tüm 'brand:{id}:branches:*' anahtarlarını silmeliyiz (Redis SCAN ile)
            // ya da bu listeyi hiç cache'lememeliyiz.
            // Şimdilik en basit olanı loglayıp geçmek:
            _logger.LogWarning("Event: {Event}. Şube listesi cache'i temizlenemedi (dinamik anahtar).", nameof(BranchCreatedDomainEvent));
            return Task.CompletedTask;

            // TODO: Redis kullanılıyorsa, "brand:{notification.BrandId}:branches:*" pattern'ı temizlenmeli.
        }

        // Şube güncellendi -> O şubenin DETAYI geçersiz oldu
        public Task Handle(BranchAddressUpdatedDomainEvent notification, CancellationToken cancellationToken)
        {
            var cacheKey = new GetBranchByIdQuery { BranchId = notification.BranchId }.CacheKey; // "branch:{id}"
            _logger.LogInformation("Event: {Event}. Cache temizleniyor: {CacheKey}", nameof(BranchAddressUpdatedDomainEvent), cacheKey);
            return ClearCacheAsync(cacheKey, cancellationToken);
        }

        // --- User Handlers ---

        // Kullanıcı profili güncellendi -> Kullanıcının "Profil Detayı" geçersiz oldu
        public Task Handle(UserProfileUpdatedDomainEvent notification, CancellationToken cancellationToken)
        {
            var cacheKey = new GetMyProfileQuery { IdentityId = notification.IdentityId }.CacheKey; // "user:identity:{id}"
            _logger.LogInformation("Event: {Event}. Cache temizleniyor: {CacheKey}", nameof(UserProfileUpdatedDomainEvent), cacheKey);
            return ClearCacheAsync(cacheKey, cancellationToken);
        }

        // Kullanıcı check-in yaptı -> Kullanıcının "Profil Detayı" (BranchId'si değiştiği için) geçersiz oldu
        public Task Handle(UserCheckedInToBranchDomainEvent notification, CancellationToken cancellationToken)
        {
            // Bu event'te IdentityId yok. Bu bir tasarım hatası.
            // ÇÖZÜM: `UserCheckedInToBranchDomainEvent`'e IdentityId'yi eklemeliyiz.
            // ŞİMDİLİK: Bu event'i de `UserProfileUpdated` gibi `IdentityId` alacak şekilde güncellediğimizi varsayalım.
            // Geçici olarak bu handler'ı atlıyoruz.

            // TODO: UserCheckedInToBranchDomainEvent'e IdentityId ekle
            //

            _logger.LogWarning("Event: {Event}. Cache temizlenemedi (IdentityId eksik).", nameof(UserCheckedInToBranchDomainEvent));
            return Task.CompletedTask;
        }

        // --- Helper Metot ---

        private async Task ClearCacheAsync(string cacheKey, CancellationToken cancellationToken)
        {
            try
            {
                await _cache.RemoveAsync(cacheKey, cancellationToken);
                _logger.LogInformation("Cache Başarıyla Temizlendi. Key: {CacheKey}", cacheKey);
            }
            catch (Exception ex)
            {
                // Cache (örn: Redis) çökerse, ana uygulama çöpmemeli.
                _logger.LogWarning(ex, "Cache Temizleme Hatası (Key: {CacheKey}). İşlem devam edecek.", cacheKey);
            }
        }
    }
}
