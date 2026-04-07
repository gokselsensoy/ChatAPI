using Application.Abstractions.QueryRepositories;
using Domain.Entities;
using Domain.SeedWork;
using MediatR;

namespace Application.Features.Menus.Commands.CreateMenu
{
    public class CreateMenuCommandHandler : IRequestHandler<CreateMenuCommand, Guid>
    {
        private readonly IRepository<Menu> _menuRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBranchQueryRepository _branchQueryRepository;

        public CreateMenuCommandHandler(
            IRepository<Menu> menuRepository,
            IUnitOfWork unitOfWork,
            IBranchQueryRepository branchQueryRepository)
        {
            _menuRepository = menuRepository;
            _unitOfWork = unitOfWork;
            _branchQueryRepository = branchQueryRepository;
        }

        public async Task<Guid> Handle(CreateMenuCommand request, CancellationToken cancellationToken)
        {
            if (!await _branchQueryRepository.CanUserManageBranchAsync(request.ActingUserId, request.BranchId, cancellationToken))
                throw new UnauthorizedAccessException("Bu şube için menü oluşturma yetkiniz yok.");

            var menu = Menu.Create(request.Title, request.Description, request.MenuType, request.BranchId, request.MenuUrl, request.FileId);

            _menuRepository.Add(menu);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return menu.Id;
        }
    }
}
