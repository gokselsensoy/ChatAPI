using Domain.Entities;
using Domain.Repositories;
using Domain.SeedWork;
using Domain.ValueObjects;
using MediatR;

namespace Application.Features.Branchs.Commands.CreateBranch
{
    public class CreateBranchCommandHandler : IRequestHandler<CreateBranchCommand, Guid>
    {
        private readonly IBranchRepository _branchRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateBranchCommandHandler(IBranchRepository branchRepository, IUnitOfWork unitOfWork)
        {
            _branchRepository = branchRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(CreateBranchCommand request, CancellationToken cancellationToken)
        {
            var address = new Address(
                request.Country, request.City, request.District, request.Neighborhood,
                request.Street, request.BuildingNumber, request.ZipCode,
                request.Latitude, request.Longitude, request.ApartmentNumber
            );

            var newBranch = Branch.Create(
                request.Name,
                request.BrandId,
                address,
                request.BranchType,
                request.FileId
            );

            _branchRepository.Add(newBranch);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return newBranch.Id;
        }
    }
}
