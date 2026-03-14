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

            // --- EKLENEN KRİTİK KISIM: Gerçek User'ı Bulmak ---
            // request.UserId şu an aslında Token'dan gelen IdentityId. 
            // IdentityId senin User tablosunda string mi Guid mi tutuluyor bilmiyorum. 
            // Eğer string ise .ToString() ekleyebilirsin: u.IdentityId == request.UserId.ToString()
            var appUser = await _userQueryRepo.GetAsync(
                u => u.IdentityId == request.UserId, // veya u.IdentityId == request.UserId.ToString()
                cancellationToken);

            if (appUser == null)
                throw new UnauthorizedAccessException("Uygulama kullanıcı profili bulunamadı.");

            // Artık veritabanı ilişkilerinde kullanacağımız GERÇEK Id bu:
            var realUserId = appUser.Id;
            // ---------------------------------------------------

            // 2. Mevcut Konumu Getir (Artık realUserId kullanıyoruz)
            var currentLocation = await _userLocationRepository.GetAsync(
                ul => ul.UserId == realUserId,
                cancellationToken);

            if (currentLocation == null)
            {
                currentLocation = UserLocation.Create(realUserId, request.BranchId);
                _userLocationRepository.Add(currentLocation);
            }
            else
            {
                currentLocation.UpdateLocation(request.BranchId);
                _userLocationRepository.Update(currentLocation);
            }

            // 3. Geçmiş (History) Kaydı Oluştur (Artık realUserId kullanıyoruz)
            var historyLog = CheckInHistory.Create(realUserId, request.BranchId);
            _historyRepository.Add(historyLog);

            // 4. Veritabanına Kaydet
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
