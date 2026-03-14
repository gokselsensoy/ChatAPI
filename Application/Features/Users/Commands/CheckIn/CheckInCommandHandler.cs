using Application.Abstractions.QueryRepositories;
using Application.Exceptions;
using Domain.Entities;
using Domain.Repositories;
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
        private readonly IUserQueryRepository _userQueryRepo;

        public CheckInCommandHandler(
            IRepository<UserLocation> userLocationRepository,
            IRepository<CheckInHistory> historyRepository,
            IRepository<Branch> branchRepository,
            IUnitOfWork unitOfWork,
            IUserQueryRepository userQueryRepo)
        {
            _userLocationRepository = userLocationRepository;
            _historyRepository = historyRepository;
            _branchRepository = branchRepository;
            _unitOfWork = unitOfWork;
            _userQueryRepo = userQueryRepo;
        }

        public async Task<bool> Handle(CheckInCommand request, CancellationToken cancellationToken)
        {
            // 1. Şube Kontrolü
            var branchExists = await _branchRepository.AnyAsync(b => b.Id == request.BranchId, cancellationToken);
            if (!branchExists)
                throw new NotFoundException("Şube bulunamadı.", request.BranchId);


            // 2. Mevcut Konumu Getir (Artık realUserId kullanıyoruz)
            var currentLocation = await _userLocationRepository.GetAsync(
                ul => ul.UserId == request.UserId,
                cancellationToken);

            if (currentLocation == null)
            {
                currentLocation = UserLocation.Create(request.UserId, request.BranchId);
                _userLocationRepository.Add(currentLocation);
            }
            else
            {
                currentLocation.UpdateLocation(request.BranchId);
                _userLocationRepository.Update(currentLocation);
            }

            // 3. Geçmiş (History) Kaydı Oluştur (Artık realUserId kullanıyoruz)
            var historyLog = CheckInHistory.Create(request.UserId, request.BranchId);
            _historyRepository.Add(historyLog);

            // 4. Veritabanına Kaydet
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
