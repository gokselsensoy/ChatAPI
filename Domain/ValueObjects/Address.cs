using Domain.SeedWork;

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
        public decimal Latitude { get; }
        public decimal Longitude { get; }

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
            Latitude = latitude;
            Longitude = longitude;
            ApartmentNumber = apartmentNumber;
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
            yield return Latitude;
            yield return Longitude;
            if (ApartmentNumber != null)
                yield return ApartmentNumber;
        }
    }
}
