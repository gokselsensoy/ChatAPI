namespace Application.Features.Menus.DTOs
{
    public class MenuDto
    {
        public Guid Id { get; set; }
        public Guid BranchId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string MenuType { get; set; }
        public string? MenuUrl { get; set; }
        public string? FileId { get; set; }

        // Menüye ait ürünler
        public List<MenuItemDto> MenuItems { get; set; } = new();
    }
}
