using Application.Exceptions;
using Domain.Entities;
using Domain.Repositories;
using Domain.SeedWork;
using MediatR;

namespace Application.Features.Menus.Commands.UpdateMenuItem
{
    public class UpdateMenuItemCommandHandler : IRequestHandler<UpdateMenuItemCommand, bool>
    {
        private readonly IMenuRepository _menuCommandRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateMenuItemCommandHandler(IMenuRepository menuCommandRepository, IUnitOfWork unitOfWork)
        {
            _menuCommandRepository = menuCommandRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(UpdateMenuItemCommand request, CancellationToken cancellationToken)
        {
            // 1. Menüyü ürünleriyle beraber çek
            var menu = await _menuCommandRepository.GetByIdWithItemsAsync(request.MenuId, cancellationToken);
            if (menu == null)
                throw new NotFoundException(nameof(Menu), request.MenuId);

            // 2. İşlemi Aggregate Root'a (Menu) devret
            // Not: UpdateItem metodunu bir önceki adımda Menu entity'sine eklemiştik
            menu.UpdateItem(
                request.MenuItemId,
                request.Name,
                request.Description,
                request.CategoryType,
                request.Price,
                request.FileId);

            // 3. Değişiklikleri kaydet
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
