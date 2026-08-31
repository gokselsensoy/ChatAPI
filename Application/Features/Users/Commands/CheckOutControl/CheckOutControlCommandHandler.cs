using Application.Abstractions.QueryRepositories;
using Application.Features.Users.Commands.CheckOut;
using Domain;
using Domain.Entities;
using Domain.Repositories;
using Domain.SeedWork;
using MediatR;
using NetTopologySuite.Geometries;

namespace Application.Features.Users.Commands.CheckOutControl
{
    public class CheckOutControlCommandHandler : IRequestHandler<CheckOutControlCommand, bool>
    {
        private readonly IRepository<UserLocation> _locationRepo;
        private readonly IRepository<Branch> _branchRepo;
        private readonly IMediator _mediator;
        private readonly IUserQueryRepository _userQueryRepo;
        private readonly IBranchQueryRepository _branchQueryRepo;

        public CheckOutControlCommandHandler(IRepository<UserLocation> locationRepo,
            IRepository<Branch> branchRepo,
            IMediator mediator,
            IUserQueryRepository userQueryRepo,
            IBranchQueryRepository branchQueryRepo) 
        {
            _locationRepo = locationRepo;
            _branchRepo = branchRepo;
            _mediator = mediator;
            _userQueryRepo = userQueryRepo;
            _branchQueryRepo = branchQueryRepo;
        }

        public async Task<bool> Handle(CheckOutControlCommand request, CancellationToken cancellationToken)
        {
            // 1. Gerçek User'ı bul
            var appUser = await _userQueryRepo.GetAsync(u => u.IdentityId == request.UserId, cancellationToken);
            if (appUser == null) return false;

            var realUserId = appUser.Id;

            // 2. Artık realUserId ile location arıyoruz
            var currentLocation = await _locationRepo.GetAsync(
                ul => ul.UserId == realUserId,
                cancellationToken);

            if (currentLocation == null) return false; // Zaten dışarıda

            var branch = await _branchRepo.GetByIdAsync(currentLocation.BranchId, cancellationToken);

            if (branch == null)
            {
                // Şube silinmişse kullanıcıyı check-out yapalım.
                // DİKKAT: CheckOutCommand IdentityId beklediği için request.UserId gönderiyoruz!
                await _mediator.Send(new CheckOutCommand { UserId = request.UserId }, cancellationToken);
                return true;
            }

            // Brand owner veya bu şubede admin olan kullanıcılar için konum tabanlı otomatik checkout yapılmaz.
            if (await _branchQueryRepo.CanUserManageBranchAsync(realUserId, currentLocation.BranchId, cancellationToken))
            {
                return false;
            }

            var userPoint = new Point((double)request.Longitude, (double)request.Latitude) { SRID = 4326 };

            double distanceInMeters = GeoConstants.DistanceInMeters(branch.Address.Location, userPoint);

            if (distanceInMeters > GeoConstants.CheckInRadiusInMeters)
            {
                // DİKKAT: CheckOutCommand IdentityId beklediği için request.UserId gönderiyoruz!
                await _mediator.Send(new CheckOutCommand { UserId = request.UserId }, cancellationToken);
                return true;
            }

            return false;
        }
    }
}
