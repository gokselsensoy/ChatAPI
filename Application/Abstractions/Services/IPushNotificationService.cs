namespace Application.Abstractions.Services
{
    public class PushMessage
    {
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public Dictionary<string, string> Data { get; set; } = new();
    }

    public interface IPushNotificationService
    {
        Task SendToTokensAsync(IReadOnlyList<string> deviceTokens, PushMessage message, CancellationToken cancellationToken = default);
    }
}
