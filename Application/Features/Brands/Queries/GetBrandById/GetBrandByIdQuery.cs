using Application.Abstractions.Messaging;
using Application.Features.Brands.DTOs;
using MediatR;

namespace Application.Features.Brands.Queries.GetBrandById
{
    public class GetBrandByIdQuery : IRequest<BrandDto>
    {
        public Guid BrandId { get; set; }
    }
}
