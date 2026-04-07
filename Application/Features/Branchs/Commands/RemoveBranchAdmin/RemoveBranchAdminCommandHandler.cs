using Application.Abstractions.QueryRepositories;
using Application.Exceptions;
using Domain.Entities;
using Domain.Repositories;
using Domain.SeedWork;
using MediatR;

namespace Application.Features.Branchs.Commands.RemoveBranchAdmin
{
    public class RemoveBranchAdminCommandHandler : IRequestHandler<RemoveBranchAdminCommand, bool>
    {
        private readonly IBranchRepository _branchRepository;
        private readonly IBranchQueryRepository _branchQueryRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RemoveBranchAdminCommandHandler(
            IBranchRepository branchRepository,
            IBranchQueryRepository branchQueryRepository,
            IUnitOfWork unitOfWork)
        {
            _branchRepository = branchRepository;
            _branchQueryRepository = branchQueryRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(RemoveBranchAdminCommand request, CancellationToken cancellationToken)
        {
            if (!await _branchQueryRepository.CanUserManageBranchAsync(request.ActingUserId, request.BranchId, cancellationToken))
                throw new UnauthorizedAccessException("Bu şube için yönetici kaldırma yetkiniz yok.");

            var branch = await _branchRepository.GetByIdWithAdminMapsAsync(request.BranchId, cancellationToken)
                ?? throw new NotFoundException(nameof(Branch), request.BranchId);

            branch.RemoveDelegatedAdmin(request.UserId);
            _branchRepository.Update(branch);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
