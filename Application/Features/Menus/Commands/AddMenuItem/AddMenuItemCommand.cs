using Application.Abstractions.Messaging;
using Domain.Enums;

namespace Application.Features.Menus.Commands.AddMenuItem
{
    public class AddMenuItemCommand : ICommand<Guid>
    {
        public Guid MenuId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public CategoryType CategoryType { get; set; }
        public decimal Price { get; set; }
        public string? FileId { get; set; }
    }
}
