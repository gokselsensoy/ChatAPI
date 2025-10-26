using Domain.Enums;
using Domain.SeedWork;

namespace Domain.Entities
{
    public class Menu : Entity, IAggregateRoot
    {
        public string Title { get; private set; }
        public string Description { get; private set; }
        public MenuType MenuType { get; private set; }
        public string? MenuUrl { get; private set; }
        public string? FileId { get; private set; }
        public Guid BranchId { get; private set; }

        // Navigations
        public Branch? Branch { get; private set; }
        public ICollection<MenuItem> MenuItems { get; private set; } = new List<MenuItem>();

        private Menu() { }
    }
}