using Domain.SeedWork;
using NetTopologySuite.Geometries;

namespace Domain.ValueObjects
{
    public class Address : ValueObject
    {
        public string Country { get; }
        public string City { get; }
        public string District { get; }
        public string Neighborhood { get; }
        public string Street { get; }
        public string BuildingNumber { get; }
        public string? ApartmentNumber { get; }
        public string ZipCode { get; }
        public Point Location { get; private set; }

        private Address() { }

        public Address(
            string country, string city, string district, string neighborhood,
            string street, string buildingNumber, string zipCode,
            decimal latitude, decimal longitude, string? apartmentNumber = null)
        {
            // Burada validasyonlar yapılabilir
            // Örn: if (latitude < -90 || latitude > 90) throw new ...
            Country = country;
            City = city;
            District = district;
            Neighborhood = neighborhood;
            Street = street;
            BuildingNumber = buildingNumber;
            ZipCode = zipCode;
            ApartmentNumber = apartmentNumber;
            Location = new Point((double)longitude, (double)latitude) { SRID = 4326 };
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Country;
            yield return City;
            yield return District;
            yield return Neighborhood;
            yield return Street;
            yield return BuildingNumber;
            yield return ZipCode;
            yield return Location;
            if (ApartmentNumber != null)
                yield return ApartmentNumber;
        }
    }
}
