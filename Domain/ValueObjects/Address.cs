using Domain.SeedWork;

namespace Domain.ValueObjects
{
    public class Address : ValueObject
    {
        public string Country { get; }
        public string Street { get; }
        public string City { get; }
        public string ZipCode { get; }
        public decimal Lat { get; }
        public decimal Long { get; }

        public Address(string country, string street, string city, string zipCode, decimal lat, decimal longProp)
        {
            Country = country;
            Street = street;
            City = city;
            ZipCode = zipCode;
            Lat = lat;
            Long = longProp;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Country;
            yield return Street;
            yield return City;
            yield return ZipCode;
            yield return Lat;
            yield return Long;
        }
    }
}
