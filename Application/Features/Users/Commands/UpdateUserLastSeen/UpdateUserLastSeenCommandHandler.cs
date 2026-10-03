using Domain.Repositories;
using Domain.SeedWork;

namespace Application.Features.Users.Commands.UpdateUserLastSeen
{
    public class UpdateUserLastSeenCommandHandler : IRequestHandler<UpdateUserLastSeenCommand, DateTime?>
    {
        private readonly IUserRespository _userRespository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateUserLastSeenCommandHandler(IUserRepository userRespository, IUnitOfWork unitOfWork)
        {
            _userRespository = userRespository;
            _unitOfWork = unitOfWork;
        }

        public async Task<DateTime?> Handle(UpdateUserLastSeenCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRespository.GetByIdAsync(request.UserId, cancellationToken);

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