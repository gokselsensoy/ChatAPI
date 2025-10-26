using Application.Abstractions.EntityRepositories;
using Application.Exceptions;
using Application.Features.Orders.Queries.Shared.DTOs;
using AutoMapper;
using Domain.Entities;
using Domain.Repositories;
using MediatR;

namespace Application.Features.Orders.Queries.GetOrderById
{
    public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto>
    {
        private readonly IOrderQueryRepository _orderQueryRepository;

        public GetOrderByIdQueryHandler(IOrderQueryRepository orderQueryRepository)
        {
            _orderQueryRepository = orderQueryRepository;
        }

        public async Task<OrderDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            var orderDto = await _orderQueryRepository.GetOrderByIdAsync(request.OrderId, cancellationToken);

            if (orderDto == null)
            {
                throw new NotFoundException(nameof(Order), request.OrderId);
            }

            return orderDto;
        }
    }
}
