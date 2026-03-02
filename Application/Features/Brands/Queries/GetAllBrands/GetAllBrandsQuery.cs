using Application.Abstractions.Messaging;
using Application.Features.Brands.DTOs;
using MediatR;

namespace Application.Features.Brands.Queries.GetAllBrands
{
    public class GetAllBrandsQuery : IRequest<List<BrandDto>>
    {
    }
}
