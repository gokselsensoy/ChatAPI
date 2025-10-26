using Application.Abstractions.Messaging;
using Application.Features.Orders.Queries.Shared.DTOs;
using MediatR;

namespace Application.Features.Orders.Queries.GetOrderById
{
    public class GetOrderByIdQuery : ICachableQuery<OrderDto>
    {
        public Guid OrderId { get; set; }
        public string CacheKey => $"order:{OrderId}";
        public TimeSpan CacheDuration => TimeSpan.FromMinutes(10);
    }
}
