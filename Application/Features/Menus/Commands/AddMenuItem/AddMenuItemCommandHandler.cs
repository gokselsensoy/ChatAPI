using Application.Abstractions.QueryRepositories;
using Application.Exceptions;
using Domain.Entities;
using Domain.Repositories;
using Domain.SeedWork;
using MediatR;

namespace Application.Features.Menus.Commands.AddMenuItem
{
    public class AddMenuItemCommandHandler : IRequestHandler<AddMenuItemCommand, Guid>
    {
        private readonly IMenuRepository _menuRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBranchQueryRepository _branchQueryRepository;

        public AddMenuItemCommandHandler(
            IMenuRepository menuRepository,
            IUnitOfWork unitOfWork,
            IBranchQueryRepository branchQueryRepository)
        {
            _menuRepository = menuRepository;
            _unitOfWork = unitOfWork;
            _branchQueryRepository = branchQueryRepository;
        }

        public async Task<Guid> Handle(AddMenuItemCommand request, CancellationToken cancellationToken)
        {
            var menu = await _menuRepository.GetByIdWithItemsAsync(request.MenuId, cancellationToken);
            if (menu == null) throw new NotFoundException(nameof(Menu), request.MenuId);

            if (!await _branchQueryRepository.CanUserManageBranchAsync(request.ActingUserId, menu.BranchId, cancellationToken))
                throw new UnauthorizedAccessException("Bu şube menüsüne ürün ekleme yetkiniz yok.");

            // 2. Aggregate Root üzerinden ürünü ekle
            var newItem = menu.AddItem(
                request.Name,
                request.Description,
                request.CategoryType,
                request.Price,
                request.FileId);

            // 3. Kaydet
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return newItem.Id;
        }
    }
}
