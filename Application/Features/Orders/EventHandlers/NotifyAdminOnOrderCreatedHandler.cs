using Application.Abstractions.Services;
using Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Orders.EventHandlers
{
    public class NotifyAdminOnOrderCreatedHandler : INotificationHandler<OrderCreatedDomainEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotifyAdminOnOrderCreatedHandler> _logger;

        public NotifyAdminOnOrderCreatedHandler(
            INotificationService notificationService,
            ILogger<NotifyAdminOnOrderCreatedHandler> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task Handle(OrderCreatedDomainEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Yeni sipariş bildirimi gönderiliyor. OrderId: {OrderId}", notification.OrderId);

            await _notificationService.SendNotificationToGroupAsync(
                "AdminDashboard",         // Hangi gruba
                "NewOrderReceived",       // İstemcinin dinleyeceği metot adı
                new { OrderId = notification.OrderId, CustomerId = notification.CustomerId }
            );

            // Ayrıca siparişi veren kullanıcıya da bildirim gönderebiliriz
            await _notificationService.SendNotificationToUserAsync(
                notification.CustomerId.ToString(), // Hangi kullanıcıya (gruba)
                "OrderConfirmed",                   // Metot adı
                new { OrderId = notification.OrderId, Message = "Siparişiniz başarıyla alındı." }
            );
        }
    }
}
