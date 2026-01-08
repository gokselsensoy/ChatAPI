using Domain.Enums;

namespace Application.Features.Branchs.DTOs
{
    public class BranchDto
    {
        public Guid Id { get; set; }
        public Guid BrandId { get; set; }
        public string Name { get; set; }
        public string? FileId { get; set; }
        public BranchType BranchType { get; set; }

        // Adres bilgileri (flattened)
        public string Country { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string Neighborhood { get; set; }
        public string Street { get; set; }
        public string BuildingNumber { get; set; }
        public string? ApartmentNumber { get; set; }
        public string ZipCode { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }
}
