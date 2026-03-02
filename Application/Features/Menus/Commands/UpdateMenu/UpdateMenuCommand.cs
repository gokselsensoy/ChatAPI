using Domain.Enums;
using MediatR;

namespace Application.Features.Menus.Commands.UpdateMenu
{
    public class UpdateMenuCommand : IRequest<bool>
    {
        public Guid MenuId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public MenuType MenuType { get; set; }
        public string? MenuUrl { get; set; }
        public string? FileId { get; set; }
    }
}
