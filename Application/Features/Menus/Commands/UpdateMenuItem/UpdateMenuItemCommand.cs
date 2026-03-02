using Domain.Enums;
using MediatR;

namespace Application.Features.Menus.Commands.UpdateMenuItem
{
    public class UpdateMenuItemCommand : IRequest<bool>
    {
        public Guid MenuId { get; set; }
        public Guid MenuItemId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public CategoryType CategoryType { get; set; }
        public decimal Price { get; set; }
        public string? FileId { get; set; }
    }
}
