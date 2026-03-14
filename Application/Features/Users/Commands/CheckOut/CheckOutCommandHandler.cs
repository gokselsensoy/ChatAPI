using Application.Abstractions.QueryRepositories;
using Domain.Entities;
using Domain.Repositories;
using Domain.SeedWork;
using MediatR;

namespace Application.Features.Users.Commands.CheckOut
{
    public class CheckOutCommandHandler : IRequestHandler<CheckOutCommand, bool>
    {
        private readonly IRepository<UserLocation> _locationRepo;
        private readonly IRepository<CheckInHistory> _historyRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserQueryRepository _userQueryRepo;

        public CheckOutCommandHandler(IRepository<UserLocation> locationRepo, 
            IRepository<CheckInHistory> historyRepo, 
            IUnitOfWork unitOfWork,
            IUserQueryRepository userQueryRepo)
        {
            _locationRepo = locationRepo;
            _historyRepo = historyRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(CheckOutCommand request, CancellationToken cancellationToken)
        {
            // 1. Gerçek User'ı bul
            var appUser = await _userQueryRepo.GetAsync(u => u.IdentityId == request.UserId, cancellationToken);
            if (appUser == null) return true; // Kullanıcı yoksa çıkış yapılmış sayabiliriz

            var realUserId = appUser.Id;

            // 2. Artık realUserId ile arıyoruz
            var location = await _locationRepo.GetAsync(
                ul => ul.UserId == realUserId,
                cancellationToken);

            if (location == null) return true;

            // 3. History'yi de realUserId ile arıyoruz
            var history = await _historyRepo.GetAsync(
                h => h.UserId == realUserId && h.CheckOutTime == null,
                cancellationToken);

            if (history != null)
            {
                history.MarkAsCheckedOut();
                _historyRepo.Update(history);
            }

            _locationRepo.Delete(location); // Update yerine Delete yazmışsın, DDD'ye göre mantıklı.

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
