using Domain.Entities;
using Domain.SeedWork;
using MediatR;

namespace Application.Features.Users.Commands.CheckOut
{
    public class CheckOutCommandHandler : IRequestHandler<CheckOutCommand, bool>
    {
        private readonly IRepository<UserLocation> _locationRepo;
        private readonly IRepository<CheckInHistory> _historyRepo;
        private readonly IUnitOfWork _unitOfWork;

        public CheckOutCommandHandler(IRepository<UserLocation> locationRepo, 
            IRepository<CheckInHistory> historyRepo, 
            IUnitOfWork unitOfWork)
        {
            _locationRepo = locationRepo;
            _historyRepo = historyRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(CheckOutCommand request, CancellationToken cancellationToken)
        {
            var location = await _locationRepo.GetAsync(
                ul => ul.UserId == request.UserId,
                cancellationToken);

            if (location == null) return true;

            var history = await _historyRepo.GetAsync(
                h => h.UserId == request.UserId && h.CheckOutTime == null,
                cancellationToken);

            if (history != null)
            {
                history.MarkAsCheckedOut();
                _historyRepo.Update(history);
            }

            _locationRepo.Delete(location);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
