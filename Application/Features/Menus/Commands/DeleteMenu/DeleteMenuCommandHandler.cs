using Application.Exceptions;
using Domain.Entities;
using Domain.SeedWork;
using MediatR;

namespace Application.Features.Menus.Commands.DeleteMenu
{
    public class DeleteMenuCommandHandler : IRequestHandler<DeleteMenuCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Menu> _menuRepository;

        public DeleteMenuCommandHandler(IUnitOfWork unitOfWork, IRepository<Menu> menuRepository)
        {
            _menuRepository = menuRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(DeleteMenuCommand request, CancellationToken cancellationToken)
        {
            var menu = await _menuRepository.GetAsync(m => m.Id == request.MenuId, cancellationToken);
            if (menu == null) throw new NotFoundException(nameof(Menu), request.MenuId);

            _menuRepository.Delete(menu); // Cascade delete açık olmalı ki item'lar da silinsin
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
