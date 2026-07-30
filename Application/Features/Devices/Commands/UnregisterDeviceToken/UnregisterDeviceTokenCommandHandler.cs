using Domain.Repositories;
using Domain.SeedWork;
using MediatR;

namespace Application.Features.Devices.Commands.UnregisterDeviceToken
{
    public class UnregisterDeviceTokenCommandHandler : IRequestHandler<UnregisterDeviceTokenCommand, bool>
    {
        private readonly IUserDeviceTokenRepository _deviceTokenRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UnregisterDeviceTokenCommandHandler(
            IUserDeviceTokenRepository deviceTokenRepository,
            IUnitOfWork unitOfWork)
        {
            _deviceTokenRepository = deviceTokenRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(UnregisterDeviceTokenCommand request, CancellationToken cancellationToken)
        {
            var existing = await _deviceTokenRepository.GetByTokenAsync(request.Token, cancellationToken);
            if (existing == null)
                return true;

            if (existing.UserId != request.UserId)
                return true;

            _deviceTokenRepository.Delete(existing);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
