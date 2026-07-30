using Domain.Entities;
using Domain.Repositories;
using Domain.SeedWork;
using MediatR;

namespace Application.Features.Devices.Commands.RegisterDeviceToken
{
    public class RegisterDeviceTokenCommandHandler : IRequestHandler<RegisterDeviceTokenCommand, bool>
    {
        private readonly IUserDeviceTokenRepository _deviceTokenRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RegisterDeviceTokenCommandHandler(
            IUserDeviceTokenRepository deviceTokenRepository,
            IUnitOfWork unitOfWork)
        {
            _deviceTokenRepository = deviceTokenRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(RegisterDeviceTokenCommand request, CancellationToken cancellationToken)
        {
            var existing = await _deviceTokenRepository.GetByTokenAsync(request.Token, cancellationToken);
            if (existing != null)
            {
                existing.Refresh(request.UserId, request.Platform);
                _deviceTokenRepository.Update(existing);
            }
            else
            {
                var device = UserDeviceToken.Create(request.UserId, request.Token, request.Platform);
                _deviceTokenRepository.Add(device);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
