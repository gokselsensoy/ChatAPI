using Domain.Enums;
using Domain.SeedWork;

namespace Domain.Entities
{
    public class MenuItem : Entity
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public CategoryType CategoryType { get; private set; }
        public decimal Price { get; private set; }
        public Guid MenuId { get; private set; }
        public string? FileId { get; private set; }

        // Navigations
        public Menu? Menu { get; private set; }

        private MenuItem() { }

        internal static MenuItem Create(Guid menuId, string name, string description, CategoryType categoryType, decimal price, string? fileId)
        {
            return new MenuItem
            {
                Id = Guid.NewGuid(),
                MenuId = menuId,
                Name = name,
                Description = description,
                CategoryType = categoryType,
                Price = price,
                FileId = fileId
            };
        }

        internal void Update(string name, string description, CategoryType categoryType, decimal price, string? fileId)
        {
            Name = name;
            Description = description;
            CategoryType = categoryType;
            Price = price;
            FileId = fileId;
        }
    }
}