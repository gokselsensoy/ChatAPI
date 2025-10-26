namespace Application.Features.Brands.DTOs
{
    public class BrandDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? FileId { get; set; }
        public Guid OwnerUserId { get; set; }
    }
}
