using Application.Abstractions.QueryRepositories;
using Application.Features.Brands.DTOs;
using MediatR;

namespace Application.Features.Brands.Queries.GetAllBrands
{
    public class GetAllBrandsQueryHandler : IRequestHandler<GetAllBrandsQuery, List<BrandDto>>
    {
        private readonly IBrandQueryRepository _brandQueryRepository;

        public GetAllBrandsQueryHandler(IBrandQueryRepository brandQueryRepository)
        {
            _brandQueryRepository = brandQueryRepository;
        }

        public async Task<List<BrandDto>> Handle(GetAllBrandsQuery request, CancellationToken cancellationToken)
        {
            return await _brandQueryRepository.GetAllAsync(cancellationToken);
        }
    }
}
