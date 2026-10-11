using Domain.Entities;
using Domain.Repositories;
using Domain.SeedWork;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace Application.Features.BranchEvents.Commands.CreateBranchEvent
{
    public class CreateBranchEventCommandHandler : IRequestHandler<CreateBranchEventCommand, Guid>
    {
        private readonly IBranchEventRepository _branchEventRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateBranchEventCommandHandler(IBranchEventRepository repository, IUnitOfWork unitOfWork)
        {
            _branchEventRepository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(CreateBranchEventCommand request, CancellationToken cancellationToken)
        {
            var branchEvent = BranchEvent.Create(
                request.BranchId,
                request.Title,
                request.Description,
                request.StartDate,
                request.EndDate,
                request.IsHighlighted,
                request.ImageUrl
            );

            _branchEventRepository.Add(branchEvent);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return branchEvent.Id;
        }
    }
}