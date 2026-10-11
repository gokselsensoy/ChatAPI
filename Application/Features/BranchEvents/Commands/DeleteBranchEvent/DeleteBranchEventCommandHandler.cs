using Application.Exceptions;
using Domain.Entities;
using Domain.Repositories;
using Domain.SeedWork;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.BranchEvents.Commands.DeleteBranchEvent
{
    public class DeleteBranchEventCommandHandler : IRequestHandler<DeleteBranchEventCommand, bool>
    {
        private readonly IBranchEventRepository _branchEventRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteBranchEventCommandHandler(IBranchEventRepository repository, IUnitOfWork unitOfWork)
        {
            _branchEventRepository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(DeleteBranchEventCommand request, CancellationToken cancellationToken)
        {
            var branchEvent = await _branchEventRepository.GetByIdAsync(request.EventId, cancellationToken);
            if (branchEvent == null || branchEvent.IsDeleted)
                throw new NotFoundException(nameof(BranchEvent), request.EventId);

            branchEvent.MarkAsDeleted();

            _branchEventRepository.Update(branchEvent);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}