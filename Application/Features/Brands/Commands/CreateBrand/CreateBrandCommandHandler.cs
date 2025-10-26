using Domain.Entities;
using Domain.Repositories;
using Domain.SeedWork;
using MediatR;

namespace Application.Features.Brands.Commands.CreateBrand
{
    public class CreateBrandCommandHandler : IRequestHandler<CreateBrandCommand, Guid>
    {
        private readonly IBrandRepository _brandRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateBrandCommandHandler(
            IBrandRepository brandRepository,
            IUnitOfWork unitOfWork)
        {
            _brandRepository = brandRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(CreateBrandCommand request, CancellationToken cancellationToken)
        {
            var newBrand = Brand.Create(
                request.Name,
                request.OwnerUserId,
                request.FileId
            );

            _brandRepository.Add(newBrand);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 4. Yeni ID'yi dön
            return newBrand.Id;
        }
    }
}
