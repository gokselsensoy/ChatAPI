using Application.Abstractions.Messaging;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Application.Pipelines
{
    public class CachingPipelineBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
            where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<CachingPipelineBehaviour<TRequest, TResponse>> _logger;
        private readonly IDistributedCache _cache; // Redis veya In-Memory Cache

        public CachingPipelineBehaviour(
            ILogger<CachingPipelineBehaviour<TRequest, TResponse>> logger,
            IDistributedCache cache)
        {
            _logger = logger;
            _cache = cache;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // İsteğin ICachableQuery arayüzünü uygulayıp uygulamadığını kontrol et
            if (request is not ICachableQuery<TResponse> cachableRequest)
            {
                // Eğer cache'lenebilir bir query değilse, direkt devam et
                return await next();
            }

            var requestName = typeof(TRequest).Name;
            var cacheKey = cachableRequest.CacheKey;

            try
            {
                // 1. Cache'den okumayı dene
                var cachedResponseString = await _cache.GetStringAsync(cacheKey, cancellationToken);

                if (!string.IsNullOrEmpty(cachedResponseString))
                {
                    _logger.LogInformation("Cache'den Getirildi: {RequestName}. Key: {CacheKey}", requestName, cacheKey);
                    // Cache'de bulundu, string'i objeye geri dönüştür (Deserialize)
                    return JsonSerializer.Deserialize<TResponse>(cachedResponseString)!;
                }
            }
            catch (Exception ex)
            {
                // Cache (örn: Redis) erişiminde hata olursa logla ama işlemi durdurma, DB'den devam et.
                _logger.LogWarning(ex, "Cache Okuma Hatası: {RequestName}. Key: {CacheKey}. DB'den devam edilecek.", requestName, cacheKey);
            }

            // 2. Cache'de yoksa, asıl Handler'ı (DB'ye giden) çalıştır
            _logger.LogInformation("Cache'de Bulunamadı, Handler Çalıştırılıyor: {RequestName}. Key: {CacheKey}", requestName, cacheKey);
            var response = await next();

            try
            {
                // 3. Gelen sonucu Cache'e yaz
                var responseString = JsonSerializer.Serialize(response);
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = cachableRequest.CacheDuration
                };

                await _cache.SetStringAsync(cacheKey, responseString, cacheOptions, cancellationToken);
                _logger.LogInformation("Cache'e Yazıldı: {RequestName}. Key: {CacheKey}", requestName, cacheKey);
            }
            catch (Exception ex)
            {
                // Cache yazma hatası olursa logla, ama kullanıcıya sonucu dön, işlemi durdurma.
                _logger.LogWarning(ex, "Cache Yazma Hatası: {RequestName}. Key: {CacheKey}.", requestName, cacheKey);
            }

            return response;
        }
    }
}
