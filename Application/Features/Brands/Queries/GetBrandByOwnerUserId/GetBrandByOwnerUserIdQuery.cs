using Application.Abstractions.Messaging;
using Application.Features.Brands.DTOs;
using MediatR;

namespace Application.Features.Brands.Queries.GetBrandByOwnerUserId
{
    public class GetBrandByOwnerUserIdQuery : IRequest<BrandDto>
    {
        public Guid OwnerUserId { get; set; }
    }
}
