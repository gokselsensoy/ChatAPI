using Domain.SeedWork;

namespace Domain.ValueObjects
{
    public class Tag : ValueObject
    {
        public string Value { get; private set; }

        // EF Core için boş constructor
        private Tag() { }

        private Tag(string value)
        {
            Value = value;
        }

        // Factory Method: Nesne sadece bu metotla, kurallardan geçerek üretilebilir.
        public static Tag Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Etiket boş olamaz.");

            var trimmedValue = value.Trim();

            if (trimmedValue.Length > 30)
                throw new ArgumentException("Etiket 30 karakterden uzun olamaz.");

            // İsteğe bağlı: Her kelimenin ilk harfini büyüt (Title Case) yapabilirsin.

            return new Tag(trimmedValue);
        }

        // Value Object eşitlik kuralı (Büyük/küçük harf duyarsız eşleştirme yapıyoruz)
        protected override IEnumerable<object> GetEqualityComponents()
        {
            // "CANLI MÜZİK" ile "canlı müzik" aynı tag sayılsın diye
            yield return Value.ToLowerInvariant();
        }

        // Kod içinde direkt string'e çevirmek kolay olsun diye
        public override string ToString() => Value;
    }
}
