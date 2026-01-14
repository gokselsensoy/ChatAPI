using Domain.Entities;
using Domain.Repositories;
using Domain.SeedWork;
using Domain.ValueObjects;
using MediatR;

namespace Application.Features.Branchs.Commands.UpdateBranch
{
    public class UpdateBranchCommandHandler : IRequestHandler<UpdateBranchCommand, Branch>
    {
        private readonly IBranchRepository _branchRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateBranchCommandHandler(IBranchRepository branchRepository, IUnitOfWork unitOfWork)
        {
            _branchRepository = branchRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Branch> Handle(UpdateBranchCommand request, CancellationToken cancellationToken)
        {
            // 1. Veritabanından nesneyi çek (Handler'ın işi budur)
            var branch = await _branchRepository.GetByIdAsync(request.BranchId, cancellationToken);

            if (branch == null)
            {
                // Global Exception Handling varsa NotFound fırlatılabilir
                throw new Exception("Branch bulunamadı");
            }

            // 2. Value Object oluştur
            var address = new Address(
                request.Country, request.City, request.District, request.Neighborhood,
                request.Street, request.BuildingNumber, request.ZipCode,
                request.Latitude, request.Longitude, request.ApartmentNumber
            );

            // 3. Domain metodunu çağır (Artık static değil, nesne üzerinden çağrılıyor)
            branch.Update(
                request.Name,
                request.BrandId,
                address,
                request.BranchType,
                request.FileId
            );

            // 4. Kaydet
            _branchRepository.Update(branch);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Not: Entity dönmek yerine DTO dönmek daha sağlıklıdır ama şimdilik böyle bırakıyorum.
            return branch;
        }
    }
}
