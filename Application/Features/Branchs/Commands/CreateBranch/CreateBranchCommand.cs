using Application.Abstractions.Messaging;
using Domain.Enums;

namespace Application.Features.Branchs.Commands.CreateBranch
{
    public class CreateBranchCommand : ICommand<Guid>
    {
        // Route'dan alınacak
        public Guid BrandId { get; set; }

        // Body'den alınacak
        public string Name { get; set; }
        public BranchType BranchType { get; set; }
        public string? FileId { get; set; }

        // Adres Bilgileri
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
