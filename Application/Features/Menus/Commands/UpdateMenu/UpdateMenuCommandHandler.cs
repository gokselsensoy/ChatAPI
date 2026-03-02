using Application.Exceptions;
using Domain.Entities;
using Domain.SeedWork;
using MediatR;

namespace Application.Features.Menus.Commands.UpdateMenu
{
    public class UpdateMenuCommandHandler : IRequestHandler<UpdateMenuCommand, bool>
    {
        private readonly IRepository<Menu> _menuRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateMenuCommandHandler(IRepository<Menu> menuRepository, IUnitOfWork unitOfWork) 
        { 
            _menuRepository = menuRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(UpdateMenuCommand request, CancellationToken cancellationToken)
        {
            var menu = await _menuRepository.GetAsync(m => m.Id == request.MenuId, cancellationToken);
            if (menu == null) throw new NotFoundException(nameof(Menu), request.MenuId);

            menu.Update(request.Title, request.Description, request.MenuType, request.MenuUrl, request.FileId);

            _menuRepository.Update(menu);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
