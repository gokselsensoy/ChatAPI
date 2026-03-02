using MediatR;

namespace Application.Features.Menus.Commands.DeleteMenu
{
    public class DeleteMenuCommand : IRequest<bool> { public Guid MenuId { get; set; } }
}
