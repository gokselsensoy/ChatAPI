using Domain.Repositories;
using Domain.SeedWork;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System;
using Application.Exceptions;

namespace Application.Features.BranchEvents.Commands.UpdateBranchEvent
{
    public class UpdateBranchEventCommandHandler : IRequestHandler<UpdateBranchEventCommand, bool>
    {
        private readonly IBranchEventRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateBranchEventCommandHandler(IBranchEventRepository repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(UpdateBranchEventCommand request, CancellationToken cancellationToken)
        {
            var branchEvent = await _repository.GetByIdAsync(request.EventId, cancellationToken);
            if (branchEvent == null) 
                throw new NotFoundException("Etkinlik bulunamadı."); // Projede tanımlı özel bir exception varsa onu kullanabilirsin (Örn: NotFoundException)

            // Nesne kendi kurallarını işleterek kendini güncelliyor
            branchEvent.Update(
                request.Title,
                request.Description,
                request.StartDate,
                request.EndDate,
                request.IsHighlighted,
                request.ImageUrl
            );

            _repository.Update(branchEvent);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}