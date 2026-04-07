using Application.Abstractions.QueryRepositories;
using Application.Exceptions;
using Domain.Entities;
using Domain.Repositories;
using Domain.SeedWork;
using MediatR;

namespace Application.Features.Menus.Commands.DeleteMenuItem
{
    public class DeleteMenuItemCommandHandler : IRequestHandler<DeleteMenuItemCommand, bool>
    {
        private readonly IMenuRepository _menuCommandRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBranchQueryRepository _branchQueryRepository;

        public DeleteMenuItemCommandHandler(
            IMenuRepository menuCommandRepository,
            IUnitOfWork unitOfWork,
            IBranchQueryRepository branchQueryRepository)
        {
            _menuCommandRepository = menuCommandRepository;
            _unitOfWork = unitOfWork;
            _branchQueryRepository = branchQueryRepository;
        }

        public async Task<bool> Handle(DeleteMenuItemCommand request, CancellationToken cancellationToken)
        {
            var menu = await _menuCommandRepository.GetByIdWithItemsAsync(request.MenuId, cancellationToken);
            if (menu == null)
                throw new NotFoundException(nameof(Menu), request.MenuId);

            if (!await _branchQueryRepository.CanUserManageBranchAsync(request.ActingUserId, menu.BranchId, cancellationToken))
                throw new UnauthorizedAccessException("Bu şube menüsünden ürün silme yetkiniz yok.");

            // 2. Aggregate Root üzerinden ürünü listeden çıkart
            // Not: RemoveItem metodunu Menu entity'sine eklemiştik
            menu.RemoveItem(request.MenuItemId);

            // 3. EF Core OnDelete(Cascade) sayesinde listeden çıkan item'ı veritabanından da siler.
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
