using Application.Exceptions;
using Domain;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Repositories;
using Domain.SeedWork;
using MediatR;
using NetTopologySuite.Geometries;

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
            var branch = await _branchRepository.GetByIdAsync(request.BranchId, cancellationToken);
            if (branch == null)
                throw new NotFoundException("Şube bulunamadı.", request.BranchId);

            var userPoint = new Point((double)request.Longitude, (double)request.Latitude) { SRID = 4326 };
            if (!GeoConstants.IsWithinCheckInRadius(branch.Address.Location, userPoint))
                throw new UserDomainException("Bu şubeye check-in yapmak için daha yakında olmalısınız.");

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
