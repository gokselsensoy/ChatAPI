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
            var address = new Address(
                request.Country, request.City, request.District, request.Neighborhood,
                request.Street, request.BuildingNumber, request.ZipCode,
                request.Latitude, request.Longitude, request.ApartmentNumber
            );

            var branch = await _branchRepository.GetByIdAsync(request.BranchId, cancellationToken);

            branch.Update(
                request.Name,
                address,
                request.BranchType,
                request.FileId
            );

            _branchRepository.Update(branch);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return branch;
        }
    }
}
