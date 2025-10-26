using Application.Exceptions;
using Domain.Entities;
using Domain.Repositories;
using Domain.SeedWork;
using MediatR;

namespace Application.Features.Brands.Commands.UpdateBrand
{
    public class UpdateBrandOwnerCommandHandler : IRequestHandler<UpdateBrandOwnerCommand>
    {
        private readonly IBrandRepository _brandRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateBrandOwnerCommandHandler(
            IBrandRepository brandRepository,
            IUnitOfWork unitOfWork)
        {
            _brandRepository = brandRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(UpdateBrandOwnerCommand request, CancellationToken cancellationToken)
        {
            // 1. Domain varlığını bul
            var brand = await _brandRepository.GetByIdAsync(request.BrandId, cancellationToken);
            if (brand == null)
            {
                throw new NotFoundException(nameof(Brand), request.BrandId);
            }

            // 2. Domain metodunu çağır (İş kuralı entity içinde)
            brand.SetOwner(request.NewOwnerUserId);

            // 3. Değişiklikleri kaydet (Bu, Domain Event'leri de tetikler)
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
