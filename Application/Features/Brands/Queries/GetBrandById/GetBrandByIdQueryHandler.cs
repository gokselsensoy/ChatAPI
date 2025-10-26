using Application.Abstractions.QueryRepositories;
using Application.Exceptions;
using Application.Features.Brands.DTOs;
using Domain.Entities;
using MediatR;

namespace Application.Features.Brands.Queries.GetBrandById
{
    public class GetBrandByIdQueryHandler : IRequestHandler<GetBrandByIdQuery, BrandDto>
    {
        private readonly IBrandQueryRepository _brandQueryRepository;

        public GetBrandByIdQueryHandler(IBrandQueryRepository brandQueryRepository)
        {
            _brandQueryRepository = brandQueryRepository;
        }

        public async Task<BrandDto> Handle(GetBrandByIdQuery request, CancellationToken cancellationToken)
        {
            var brandDto = await _brandQueryRepository.GetByIdAsync(request.BrandId, cancellationToken);

            if (brandDto == null)
            {
                throw new NotFoundException(nameof(Brand), request.BrandId);
            }

            return brandDto;
        }
    }
}
