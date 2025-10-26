using Domain.Repositories;
using Domain.SeedWork;
using Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Orders.Commands.UpdateAddress
{
    public class UpdateOrderAddressCommandHandler : IRequestHandler<UpdateOrderAddressCommand>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateOrderAddressCommandHandler> _logger;

        // Constructor'dan IDistributedCache'i kaldırdık.
        public UpdateOrderAddressCommandHandler(
            IOrderRepository orderRepository,
            IUnitOfWork unitOfWork,
            ILogger<UpdateOrderAddressCommandHandler> logger)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task Handle(UpdateOrderAddressCommand request, CancellationToken cancellationToken)
        {
            // 1. TransactionPipelineBehaviour bizim için transaction'ı başlattı.

            _logger.LogDebug("Handler başladı: {CommandName}, OrderId: {OrderId}", nameof(UpdateOrderAddressCommand), request.OrderId);

            // 2. Domain varlığını bul
            var order = await _orderRepository.GetByIdAsync(request.OrderId);
            if (order == null)
            {
                throw new Exception($"Sipariş bulunamadı: {request.OrderId}");
            }

            // 3. Domain metodunu çağır (Bu metot OrderUpdatedDomainEvent'i fırlatacak)
            var newAddress = new Address(request.Street, request.City, request.ZipCode);
            order.UpdateShippingAddress(newAddress);

            // 4. Değişiklikleri kaydet
            // ApplicationDbContext.SaveChangesAsync() çağrıldığında:
            //    a) Değişiklikler DB'ye gider.
            //    b) Domain event'leri yakalanır ve MediatR'a (IPublisher) gönderilir.
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 5. TransactionPipelineBehaviour bizim için transaction'ı Commit edecek.

            // 6. MediatR, fırlatılan event'i OrderCacheInvalidationEventHandler'a
            //    gönderecek ve cache *otomatik olarak* temizlenecek.

            _logger.LogDebug("Handler bitti: {CommandName}, OrderId: {OrderId}", nameof(UpdateOrderAddressCommand), request.OrderId);
        }
    }
}
