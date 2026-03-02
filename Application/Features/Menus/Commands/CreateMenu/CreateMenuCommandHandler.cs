using Domain.Entities;
using Domain.SeedWork;
using MediatR;

namespace Application.Features.Menus.Commands.CreateMenu
{
    public class CreateMenuCommandHandler : IRequestHandler<CreateMenuCommand, Guid>
    {
        private readonly IRepository<Menu> _menuRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateMenuCommandHandler(IRepository<Menu> menuRepository, IUnitOfWork unitOfWork)
        {
            _menuRepository = menuRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(CreateMenuCommand request, CancellationToken cancellationToken)
        {
            var menu = Menu.Create(request.Title, request.Description, request.MenuType, request.BranchId, request.MenuUrl, request.FileId);

            _menuRepository.Add(menu);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return menu.Id;
        }
    }
}
