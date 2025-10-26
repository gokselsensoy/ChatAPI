using Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Orders.EventHandlers
{
    // INotificationHandler<T>, MediatR'ın event handler'ıdır.
    public class OrderCreatedDomainEventHandler : INotificationHandler<OrderCreatedDomainEvent>
    {
        private readonly ILogger<OrderCreatedDomainEventHandler> _logger;
        // Buraya IEmailService, IStockService gibi servisleri inject edebilirsin.

        public OrderCreatedDomainEventHandler(ILogger<OrderCreatedDomainEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(OrderCreatedDomainEvent notification, CancellationToken cancellationToken)
        {
            // Bu metot, DbContext.SaveChangesAsync çağrıldıktan SONRA çalışır.
            _logger.LogInformation("Domain Event: OrderCreatedDomainEvent yakalandı. Order ID: {OrderId}", notification.OrderId);

            // TODO: Müşteriye e-posta gönder
            // _emailService.SendOrderConfirmationEmail(notification.CustomerId, notification.OrderId);

            // TODO: Stok servisine bildirimde bulun
            // _stockService.NotifyOrderPlaced(notification.OrderId);

            return Task.CompletedTask;
        }
    }
}
