using Domain.Repositories;
using Domain.SeedWork;
using MediatR;

namespace Application.Features.Users.Commands.UpdateUserLastSeen
{
    public class UpdateUserLastSeenCommandHandler : IRequestHandler<UpdateUserLastSeenCommand, DateTime?>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateUserLastSeenCommandHandler(IUserRepository userRespository, IUnitOfWork unitOfWork)
        {
            _userRepository = userRespository;
            _unitOfWork = unitOfWork;
        }

        public async Task<DateTime?> Handle(UpdateUserLastSeenCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);

            if (user != null)
            {
                user.TouchLastSeen();

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                
                return user.LastSeenAt;
            }

            return null;
        }
    }
}