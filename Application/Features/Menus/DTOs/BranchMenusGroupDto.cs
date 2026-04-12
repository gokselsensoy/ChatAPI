namespace Application.Features.Menus.DTOs
{
    /// <summary>
    /// Marka sahibi için: şube bazında gruplanmış menüler (ürünler dahil).
    /// </summary>
    public class BranchMenusGroupDto
    {
        public Guid BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;

        public Guid BrandId { get; set; }
        public string BrandName { get; set; } = string.Empty;

        public List<MenuDto> Menus { get; set; } = new();
    }
}
