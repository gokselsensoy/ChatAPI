namespace Application.Features.Branchs.DTOs
{
    public class NearbyBranchDto
    {
        // BranchDto'dan gelen tüm alanlar
        public Guid Id { get; set; }
        public Guid BrandId { get; set; }
        public string Name { get; set; }
        public string? FileId { get; set; }
        public string BranchType { get; set; }
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
        
        public bool ActivePool { get; set; }
        public bool ActiveEvent { get; set; }

        public int FullnessLevel { get; set; }
        public string FullnessLabel { get; set; }

        public double DistanceInMeters { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
    }
}
