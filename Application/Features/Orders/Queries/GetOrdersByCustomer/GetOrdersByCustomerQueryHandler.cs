using Application.Abstractions.EntityRepositories;
using Application.Features.Orders.Queries.Shared.DTOs;
using Application.Features.Orders.Queries.Shared.Pagination;
using MediatR;

namespace Application.Features.Orders.Queries.GetOrdersByCustomer
{
    public class GetOrdersByCustomerQueryHandler : IRequestHandler<GetOrdersByCustomerQuery, PaginatedResponse<OrderDto>>
    {
        private readonly IOrderQueryRepository _orderQueryRepository;

        public GetOrdersByCustomerQueryHandler(IOrderQueryRepository orderQueryRepository)
        {
            _orderQueryRepository = orderQueryRepository;
        }

        public async Task<PaginatedResponse<OrderDto>> Handle(GetOrdersByCustomerQuery request, CancellationToken cancellationToken)
        {
            return await _orderQueryRepository.GetOrdersByCustomerAsync(request, cancellationToken);
        }
    }
}
