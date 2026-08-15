namespace Application.Options
{
    public class ExpoPushOptions
    {
        public const string SectionName = "Expo";

        /// <summary>false ise push gönderilmez.</summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Opsiyonel. Expo dashboard → Access tokens.
        /// https://docs.expo.dev/push-notifications/sending-notifications/#additional-security
        /// </summary>
        public string? AccessToken { get; set; }
    }
}
