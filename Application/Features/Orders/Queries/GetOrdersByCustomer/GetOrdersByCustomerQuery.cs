using Application.Features.Orders.Queries.Shared.DTOs;
using Application.Features.Orders.Queries.Shared.Pagination;
using MediatR;

namespace Application.Features.Orders.Queries.GetOrdersByCustomer
{
    public class GetOrdersByCustomerQuery : PaginatedRequest, IRequest<PaginatedResponse<OrderDto>>
    {
        public Guid CustomerId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
