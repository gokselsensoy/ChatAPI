using Application.Abstractions.QueryRepositories;
using Application.Exceptions;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Repositories;
using Domain.SeedWork;
using MediatR;

namespace Application.Features.Branchs.Commands.AssignBranchAdmin
{
    public class AssignBranchAdminCommandHandler : IRequestHandler<AssignBranchAdminCommand, bool>
    {
        private readonly IBranchRepository _branchRepository;
        private readonly IBranchQueryRepository _branchQueryRepository;
        private readonly IUserQueryRepository _userQueryRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AssignBranchAdminCommandHandler(
            IBranchRepository branchRepository,
            IBranchQueryRepository branchQueryRepository,
            IUserQueryRepository userQueryRepository,
            IUnitOfWork unitOfWork)
        {
            _branchRepository = branchRepository;
            _branchQueryRepository = branchQueryRepository;
            _userQueryRepository = userQueryRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(AssignBranchAdminCommand request, CancellationToken cancellationToken)
        {
            if (!await _branchQueryRepository.CanUserManageBranchAsync(request.ActingUserId, request.BranchId, cancellationToken))
                throw new UnauthorizedAccessException("Bu şube için yönetici atama yetkiniz yok.");

            var branch = await _branchRepository.GetByIdWithAdminMapsAsync(request.BranchId, cancellationToken)
                ?? throw new NotFoundException(nameof(Branch), request.BranchId);

            var targetUser = await _userQueryRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (targetUser == null)
                throw new NotFoundException(nameof(User), request.UserId);

            var ownerId = await _branchQueryRepository.GetBrandOwnerUserIdForBranchAsync(request.BranchId, cancellationToken);
            if (ownerId.HasValue && ownerId.Value == request.UserId)
                throw new BranchDomainException("Marka sahibi zaten bu şubede tam yetkilidir; ayrıca atanmış yönetici olarak eklenemez.");

            branch.AssignDelegatedAdmin(request.UserId);
            _branchRepository.Update(branch);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
