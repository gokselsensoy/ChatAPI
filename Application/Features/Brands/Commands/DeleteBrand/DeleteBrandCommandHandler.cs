using Domain.Repositories;
using Domain.SeedWork;
using MediatR;

namespace Application.Features.Brands.Commands.DeleteBrand
{
    public class DeleteBrandCommandHandler : IRequestHandler<DeleteBrandCommand>
    {
        private readonly IBrandRepository _brandRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteBrandCommandHandler(
            IBrandRepository brandRepository,
            IUnitOfWork unitOfWork)
        {
            _brandRepository = brandRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(DeleteBrandCommand request, CancellationToken cancellationToken)
        {
            // 1. Domain varlığını bul
            var brand = await _brandRepository.GetByIdAsync(request.BrandId, cancellationToken);
            if (brand == null)
            {
                // Zaten yoksa bir şey yapma (veya NotFoundException fırlat)
                return;
            }

            // 2. Repository aracılığıyla sil
            _brandRepository.Delete(brand);

            // 3. Değişiklikleri kaydet
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
