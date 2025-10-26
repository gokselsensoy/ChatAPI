using Application.Abstractions.EntityRepositories;
using Application.Features.Orders.Queries.GetOrdersByCustomer;
using Application.Features.Orders.Queries.Shared.DTOs;
using Application.Shared.Pagination;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.QueryRepositories
{
    public class OrderQueryRepository : IOrderQueryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public OrderQueryRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<OrderDto?> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            // Veritabanına optimize sorgu atar (sadece DTO'daki alanları seçer)
            return await _context.Orders
                .Where(o => o.Id == orderId)
                .ProjectTo<OrderDto>(_mapper.ConfigurationProvider, cancellationToken)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<PaginatedResponse<OrderDto>> GetOrdersByCustomerAsync(
            GetOrdersByCustomerQuery request,
            CancellationToken cancellationToken = default)
        {
            // Temel sorguyu (IQueryable) oluştur
            var query = _context.Orders
                .Where(o => o.CustomerId == request.CustomerId);

            // Dinamik filtreleri ekle
            if (request.StartDate.HasValue)
            {
                query = query.Where(o => o.CreatedDate >= request.StartDate.Value);
            }
            if (request.EndDate.HasValue)
            {
                query = query.Where(o => o.CreatedDate <= request.EndDate.Value);
            }

            // Sorguyu DTO'ya yansıt (ProjectTo)
            var dtoQuery = query
                .OrderByDescending(o => o.CreatedDate)
                .ProjectTo<OrderDto>(_mapper.ConfigurationProvider, cancellationToken);

            // Sayfalama işlemini (Count, Skip, Take) veritabanında yap
            var count = await dtoQuery.CountAsync(cancellationToken);
            var items = await dtoQuery
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            // Sonucu PaginatedResponse DTO'su olarak dön
            return new PaginatedResponse<OrderDto>(items, count, request.PageNumber, request.PageSize);
        }
    }
}
