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

        private readonly List<MenuItem> _menuItems = new();
        public IReadOnlyCollection<MenuItem> MenuItems => _menuItems.AsReadOnly();

        private Menu() { }

        public static Menu Create(string title, string description, MenuType menuType, Guid branchId, string? menuUrl = null, string? fileId = null)
        {
            return new Menu
            {
                Id = Guid.NewGuid(),
                Title = title,
                Description = description,
                MenuType = menuType,
                BranchId = branchId,
                MenuUrl = menuUrl,
                FileId = fileId
            };
        }

        public void Update(string title, string description, MenuType menuType, string? menuUrl, string? fileId)
        {
            Title = title;
            Description = description;
            MenuType = menuType;
            MenuUrl = menuUrl;
            FileId = fileId;
        }

        // --- ITEM YÖNETİMİ (Admin İşlemleri) ---

        public MenuItem AddItem(string name, string description, CategoryType categoryType, decimal price, string? fileId = null)
        {
            var item = MenuItem.Create(this.Id, name, description, categoryType, price, fileId);
            _menuItems.Add(item);
            return item;
        }

        public void RemoveItem(Guid itemId)
        {
            var item = _menuItems.FirstOrDefault(x => x.Id == itemId);
            if (item != null)
            {
                _menuItems.Remove(item);
            }
        }

        public void UpdateItem(Guid itemId, string name, string description, CategoryType categoryType, decimal price, string? fileId = null)
        {
            var item = _menuItems.FirstOrDefault(x => x.Id == itemId);
            if (item == null) throw new Exception("Ürün bulunamadı.");

            item.Update(name, description, categoryType, price, fileId);
        }
    }
}