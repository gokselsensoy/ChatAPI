using Application.Features.Users.Commands.CheckOut;
using Domain.Entities;
using Domain.SeedWork;
using MediatR;
using NetTopologySuite.Geometries;

namespace Application.Features.Users.Commands.CheckOutControl
{
    public class CheckOutControlCommandHandler : IRequestHandler<CheckOutControlCommand, bool>
    {
        private readonly IRepository<UserLocation> _locationRepo;
        private readonly IRepository<Branch> _branchRepo;
        private readonly IMediator _mediator; // CheckOutCommand'i çağırmak için

        public CheckOutControlCommandHandler(IRepository<UserLocation> locationRepo,
            IRepository<Branch> branchRepo,
            IMediator mediator) 
        {
            _locationRepo = locationRepo;
            _branchRepo = branchRepo;
            _mediator = mediator;
        }

        public async Task<bool> Handle(CheckOutControlCommand request, CancellationToken cancellationToken)
        {
            var currentLocation = await _locationRepo.GetAsync(
                ul => ul.UserId == request.UserId,
                cancellationToken);
            if (currentLocation == null) return false; // Zaten dışarıda

            var branch = await _branchRepo.GetByIdAsync(currentLocation.BranchId, cancellationToken);

            if (branch == null)
            {
                // Şube silinmişse kullanıcıyı da check-out yapalım (Cleanup)
                await _mediator.Send(new CheckOutCommand { UserId = request.UserId }, cancellationToken);
                return true;
            }


            var userPoint = new Point((double)request.Longitude, (double)request.Latitude) { SRID = 4326 };

            double distanceInMeters = branch.Address.Location.Distance(userPoint) * 111195;

            if (distanceInMeters > 100)
            {
                await _mediator.Send(new CheckOutCommand { UserId = request.UserId }, cancellationToken);
                return true;
            }

            return false;
        }
    }
}
