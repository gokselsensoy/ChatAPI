using Domain.Repositories;
using Domain.SeedWork;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System;
using Application.Exceptions;

namespace Application.Features.BranchEvents.Commands.DeleteBranchEvent
{
    public class DeleteBranchEventCommandHandler : IRequestHandler<DeleteBranchEventCommand, bool>
    {
        private readonly IBranchEventRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteBranchEventCommandHandler(IBranchEventRepository repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(DeleteBranchEventCommand request, CancellationToken cancellationToken)
        {
            var branchEvent = await _repository.GetByIdAsync(request.EventId, cancellationToken);
            if (branchEvent == null)
                throw new NotFoundException("Etkinlik bulunamadı.");

            // Entity'nin Soft Delete mantığı çalıştırılıyor
            branchEvent.MarkAsDeleted();

            _repository.Update(branchEvent);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}