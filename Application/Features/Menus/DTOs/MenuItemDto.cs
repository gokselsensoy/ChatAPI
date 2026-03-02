namespace Application.Features.Menus.DTOs
{
    public class MenuItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CategoryType { get; set; } // Enum yerine string dönmek UI için daha iyidir
        public decimal Price { get; set; }
        public string? FileId { get; set; }
    }
}
