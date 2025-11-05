using Application.Exceptions;
using Domain.Entities;
using Domain.Repositories;
using Domain.SeedWork;
using MediatR;

namespace Application.Features.Users.Commands.SetUserCurrentBranch
{
    public class SetUserCurrentBranchCommandHandler : IRequestHandler<SetUserCurrentBranchCommand>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public SetUserCurrentBranchCommandHandler(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(SetUserCurrentBranchCommand request, CancellationToken cancellationToken)
        {
            // 1. Domain varlığını LOKAL ID'sine (PKey) göre bul
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                // NotFoundException, GlobalExceptionHandlingMiddleware
                // tarafından 404'e çevrilir.
                throw new NotFoundException(nameof(User), request.UserId);
            }

            user.CheckInToBranch(request.NewBranchId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
