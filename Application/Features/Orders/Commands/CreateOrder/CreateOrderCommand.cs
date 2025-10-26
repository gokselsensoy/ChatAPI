using Application.Abstractions.Messaging;
using Application.Features.Orders.Queries.Shared.DTOs;

namespace Application.Features.Orders.Commands.CreateOrder
{
    public class CreateOrderCommand : ICommand<Guid>
    {
        public Guid CustomerId { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public string? PhotoBase64 { get; set; }
        public List<OrderItemDto> OrderItems { get; set; } = new();
    }
}
