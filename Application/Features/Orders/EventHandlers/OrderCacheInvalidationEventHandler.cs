using Domain.Events.OrderEvents;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Application.Features.Orders.EventHandlers
{
    public class OrderCacheInvalidationEventHandler :
            INotificationHandler<OrderCreatedDomainEvent>, // Yaratıldığında
            INotificationHandler<OrderUpdatedDomainEvent>  // Güncellendiğinde
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<OrderCacheInvalidationEventHandler> _logger;

        public OrderCacheInvalidationEventHandler(
            IDistributedCache cache,
            ILogger<OrderCacheInvalidationEventHandler> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        // 1. YENİ BİR SİPARİŞ OLUŞTURULDUĞUNDA ÇALIŞIR
        public async Task Handle(OrderCreatedDomainEvent notification, CancellationToken cancellationToken)
        {
            // Yeni bir sipariş eklendiğinde, "tüm siparişleri" listeleyen
            // cache'ler geçersiz olur.

            _logger.LogDebug("OrderCreatedEvent yakalandı. Cache temizleniyor: orders:all");

            // "orders:all" cache'ini temizle
            await ClearCacheAsync("orders:all", cancellationToken);

            // Belki "müşterinin siparişleri" listesi de vardır?
            await ClearCacheAsync($"orders:user:{notification.CustomerId}", cancellationToken);
        }

        // 2. MEVCUT BİR SİPARİŞ GÜNCELLENDİĞİNDE ÇALIŞIR
        public async Task Handle(OrderUpdatedDomainEvent notification, CancellationToken cancellationToken)
        {
            // Bir sipariş güncellendiğinde, hem o siparişin *detay* cache'i
            // hem de *listeleme* cache'leri geçersiz olur.

            _logger.LogDebug("OrderUpdatedEvent yakalandı. Cache temizleniyor: order:{OrderId} ve orders:all", notification.OrderId);

            // "GetOrderByIdQuery"nin oluşturduğu spesifik sipariş cache'ini temizle
            await ClearCacheAsync($"order:{notification.OrderId}", cancellationToken);

            // "GetAllOrdersQuery"nin oluşturduğu listeyi temizle
            await ClearCacheAsync("orders:all", cancellationToken);

            // Müşteriye özel listeyi temizle
            await ClearCacheAsync($"orders:user:{notification.CustomerId}", cancellationToken);
        }

        // Cache temizleme işlemini merkezi hale getiren yardımcı metot
        private async Task ClearCacheAsync(string cacheKey, CancellationToken cancellationToken)
        {
            try
            {
                await _cache.RemoveAsync(cacheKey, cancellationToken);
                _logger.LogDebug("Cache Başarıyla Temizlendi. Key: {CacheKey}", cacheKey);
            }
            catch (Exception ex)
            {
                // Cache sunucusuna (örn: Redis) erişilemezse bile ana işlem
                // durmamalı. Sadece uyarı log'u atıp devam etmeliyiz.
                _logger.LogWarning(ex, "Cache Temizleme Hatası (Key: {CacheKey}). İşlem devam edecek.", cacheKey);
            }
        }
    }
}
