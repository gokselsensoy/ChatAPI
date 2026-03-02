using Application.Exceptions;
using Domain.Entities;
using Domain.SeedWork;
using MediatR;

namespace Application.Features.Menus.Commands.AddMenuItem
{
    public class AddMenuItemCommandHandler : IRequestHandler<AddMenuItemCommand, Guid>
    {
        private readonly IRepository<Menu> _menuRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AddMenuItemCommandHandler(IRepository<Menu> menuRepository, IUnitOfWork unitOfWork)
        {
            _menuRepository = menuRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(AddMenuItemCommand request, CancellationToken cancellationToken)
        {
            // 1. Menüyü bul (Item'ları Include etmeye gerek var mı? Remove/Update için evet, Add için Repository yapına bağlı)
            var menu = await _menuRepository.GetByIdAsync(request.MenuId, cancellationToken);
            if (menu == null) throw new NotFoundException(nameof(Menu), request.MenuId);

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
