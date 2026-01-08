using Application.Abstractions.QueryRepositories;
using Application.Exceptions;
using Application.Features.Brands.DTOs;
using Domain.Entities;
using MediatR;

namespace Application.Features.Brands.Queries.GetBrandByOwnerUserId
{
    public class GetBrandByOwnerUserIdQueryHandler : IRequestHandler<GetBrandByOwnerUserIdQuery, BrandDto>
    {
        private readonly IBrandQueryRepository _brandQueryRepository;

        public GetBrandByOwnerUserIdQueryHandler(IBrandQueryRepository brandQueryRepository)
        {
            _brandQueryRepository = brandQueryRepository;
        }

        public async Task<BrandDto> Handle(GetBrandByOwnerUserIdQuery request, CancellationToken cancellationToken)
        {
            var brandDto = await _brandQueryRepository.GetByOwnerUserIdAsync(request.OwnerUserId, cancellationToken);

            if (brandDto == null)
            {
                throw new NotFoundException(nameof(Brand), request.OwnerUserId);
            }

            return brandDto;
        }
    }
}
