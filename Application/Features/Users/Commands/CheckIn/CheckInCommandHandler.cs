using Application.Exceptions;
using Domain.Entities;
using Domain.SeedWork;
using MediatR;

namespace Application.Features.Users.Commands.CheckIn
{
    public class CheckInCommandHandler : IRequestHandler<CheckInCommand, bool>
    {
        private readonly IRepository<UserLocation> _userLocationRepository;
        private readonly IRepository<CheckInHistory> _historyRepository;
        private readonly IRepository<Branch> _branchRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CheckInCommandHandler(
            IRepository<UserLocation> userLocationRepository,
            IRepository<CheckInHistory> historyRepository,
            IRepository<Branch> branchRepository,
            IUnitOfWork unitOfWork)
        {
            _userLocationRepository = userLocationRepository;
            _historyRepository = historyRepository;
            _branchRepository = branchRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(CheckInCommand request, CancellationToken cancellationToken)
        {
            var branchExists = await _branchRepository.AnyAsync(b => b.Id == request.BranchId, cancellationToken);
            if (!branchExists)
                throw new NotFoundException("Şube bulunamadı.", request.BranchId);

            var currentLocation = await _userLocationRepository.GetAsync(
                ul => ul.UserId == request.UserId,
                cancellationToken);

            if (currentLocation == null)
            {
                currentLocation = UserLocation.Create(request.UserId, request.BranchId);

                // DÜZELTME: await kaldırıldı
                _userLocationRepository.Add(currentLocation);
            }
            else
            {
                currentLocation.UpdateLocation(request.BranchId);

                // DÜZELTME: await kaldırıldı
                _userLocationRepository.Update(currentLocation);
            }

            var historyLog = CheckInHistory.Create(request.UserId, request.BranchId);

            // DÜZELTME: await kaldırıldı
            _historyRepository.Add(historyLog);

            // KRİTİK NOKTA: Veritabanı işlemi burada yapılır, burası mutlaka await olmalı!
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
