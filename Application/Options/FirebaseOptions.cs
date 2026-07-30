namespace Application.Options
{
    public class FirebaseOptions
    {
        public const string SectionName = "Firebase";

        /// <summary>Service account JSON dosya yolu (WebApi köküne göre veya absolute).</summary>
        public string? CredentialsPath { get; set; }

        /// <summary>false ise push gönderilmez (dev ortamı).</summary>
        public bool Enabled { get; set; } = false;
    }
}
