using Application.Abstractions.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Repositories;
using Domain.SeedWork;
using Domain.ValueObjects;
using Hangfire;
using MediatR;

namespace Application.Features.Orders.Commands.CreateOrder
{
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Guid>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public CreateOrderCommandHandler(
            IOrderRepository orderRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IBackgroundJobClient backgroundJobClient)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _backgroundJobClient = backgroundJobClient;
        }
        public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            // 1. Domain validasyonu için Value Object oluştur
            var shippingAddress = new Address(request.Street, request.City, request.ZipCode);

            // 2. Aggregate Root'u (Order) factory metoduyla oluştur
            var newOrder = Order.Create(request.CustomerId, shippingAddress);

            // 3. Domain kurallarını (iş mantığını) çalıştır
            foreach (var item in request.OrderItems)
            {
                newOrder.AddOrderItem(item.ProductId, item.Quantity, item.Price);
            }

            // 4. Repository ile veritabanına (in-memory) ekle
            _orderRepository.Add(newOrder);

            // 5. Fotoğraf varsa, işi sıraya al
            if (!string.IsNullOrEmpty(request.PhotoBase64))
            {
                // Bu metot anında döner, DB'ye (aynı transaction içinde) işi kaydeder
                _backgroundJobClient.Enqueue<IPhotoUploader>(
                    uploader => uploader.UploadOrderPhotoAsync(
                        newOrder.Id,
                        request.PhotoBase64,
                        newOrder.CustomerId
                    )
                );
            }

            // 6. UnitOfWork ile tüm değişiklikleri tek bir transaction'da kaydet
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 7. Sonucu dön
            return newOrder.Id;
        }
    }
}
