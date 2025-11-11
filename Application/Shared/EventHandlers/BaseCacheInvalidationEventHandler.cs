using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Application.Shared.EventHandlers
{
    public abstract class BaseCacheInvalidationEventHandler
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger _logger;

        protected BaseCacheInvalidationEventHandler(
            IDistributedCache cache,
            ILogger logger) // (ILogger<T> yerine ILogger kullanmak daha kolay kalıtım sağlar)
        {
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Belirtilen cache anahtarını loglama ve hata yakalama ile güvenli bir şekilde temizler.
        /// </summary>
        protected async Task ClearCacheAsync(string cacheKey, CancellationToken cancellationToken)
        {
            try
            {
                await _cache.RemoveAsync(cacheKey, cancellationToken);
                _logger.LogInformation("Cache Başarıyla Temizlendi. Key: {CacheKey}", cacheKey);
            }
            catch (Exception ex)
            {
                // Cache sunucusuna erişilemezse bile ana uygulama çöpmemeli.
                _logger.LogWarning(ex, "Cache Temizleme Hatası (Key: {CacheKey}). İşlem devam edecek.", cacheKey);
            }
        }
    }
}
