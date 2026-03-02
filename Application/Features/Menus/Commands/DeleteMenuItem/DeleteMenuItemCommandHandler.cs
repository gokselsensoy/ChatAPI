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

        public DeleteMenuItemCommandHandler(IMenuRepository menuCommandRepository, IUnitOfWork unitOfWork)
        {
            _menuCommandRepository = menuCommandRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(DeleteMenuItemCommand request, CancellationToken cancellationToken)
        {
            // 1. Menüyü ürünleriyle beraber çek
            var menu = await _menuCommandRepository.GetByIdWithItemsAsync(request.MenuId, cancellationToken);
            if (menu == null)
                throw new NotFoundException(nameof(Menu), request.MenuId);

            // 2. Aggregate Root üzerinden ürünü listeden çıkart
            // Not: RemoveItem metodunu Menu entity'sine eklemiştik
            menu.RemoveItem(request.MenuItemId);

            // 3. EF Core OnDelete(Cascade) sayesinde listeden çıkan item'ı veritabanından da siler.
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
