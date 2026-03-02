using MediatR;

namespace Application.Features.Menus.Commands.DeleteMenuItem
{
    public class DeleteMenuItemCommand : IRequest<bool>
    {
        public Guid MenuId { get; set; }
        public Guid MenuItemId { get; set; }
    }
}
