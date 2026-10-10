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
        private readonly IBranchEventRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateBranchEventCommandHandler(IBranchEventRepository repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(CreateBranchEventCommand request, CancellationToken cancellationToken)
        {
            // Entity içerisindeki Factory metodu çağrılıyor. (Domain Event'i kendi içinde fırlatıyor)
            var branchEvent = BranchEvent.Create(
                request.BranchId,
                request.Title,
                request.Description,
                request.StartDate,
                request.EndDate,
                request.IsHighlighted,
                request.ImageUrl
            );

            await _repository.AddAsync(branchEvent);
            
            // UnitOfWork save işleminde, Entity içinde biriken Domain Event'ler otomatik yakalanıp Publish edilecek
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return branchEvent.Id;
        }
    }
}