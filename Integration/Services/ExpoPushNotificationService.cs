using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Abstractions.Services;
using Application.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Integration.Services
{
    /// <summary>
    /// Expo Push API (https://exp.host/--/api/v2/push/send).
    /// Resmi HTTP; glyphard NuGet güncel değil (access token yok).
    /// </summary>
    public class ExpoPushNotificationService : IPushNotificationService
    {
        public const string PushSendUrl = "https://exp.host/--/api/v2/push/send";
        private const int MaxBatchSize = 100;

        private readonly HttpClient _httpClient;
        private readonly ExpoPushOptions _options;
        private readonly ILogger<ExpoPushNotificationService> _logger;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public ExpoPushNotificationService(
            HttpClient httpClient,
            IOptions<ExpoPushOptions> options,
            ILogger<ExpoPushNotificationService> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;
        }

        public async Task SendToTokensAsync(
            IReadOnlyList<string> deviceTokens,
            PushMessage message,
            CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled)
            {
                _logger.LogDebug("Expo push disabled; skipped.");
                return;
            }

            var tokens = deviceTokens
                .Where(IsExpoPushToken)
                .Distinct()
                .ToList();

            if (tokens.Count == 0)
            {
                _logger.LogDebug("No valid Expo push tokens; skipped.");
                return;
            }

            foreach (var batch in tokens.Chunk(MaxBatchSize))
            {
                var payload = batch.Select(token => new ExpoPushRequest
                {
                    To = token,
                    Title = message.Title,
                    Body = message.Body,
                    Data = message.Data.Count == 0 ? null : message.Data,
                    Sound = "default",
                    Priority = "high",
                    ChannelId = "default"
                }).ToList();

                try
                {
                    using var request = new HttpRequestMessage(HttpMethod.Post, PushSendUrl)
                    {
                        Content = JsonContent.Create(payload, options: JsonOptions)
                    };
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");

                    if (!string.IsNullOrWhiteSpace(_options.AccessToken))
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.AccessToken);

                    using var response = await _httpClient.SendAsync(request, cancellationToken);
                    var body = await response.Content.ReadAsStringAsync(cancellationToken);

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("Expo push HTTP {Status}: {Body}", (int)response.StatusCode, body);
                        continue;
                    }

                    LogTicketErrors(body);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Expo push send failed");
                }
            }
        }

        public static bool IsExpoPushToken(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return false;

            var t = token.Trim();
            return t.StartsWith("ExponentPushToken[", StringComparison.Ordinal)
                || t.StartsWith("ExpoPushToken[", StringComparison.Ordinal);
        }

        private void LogTicketErrors(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (!doc.RootElement.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array)
                    return;

                var errors = 0;
                foreach (var ticket in data.EnumerateArray())
                {
                    if (ticket.TryGetProperty("status", out var status)
                        && status.GetString() == "error")
                    {
                        errors++;
                        var msg = ticket.TryGetProperty("message", out var m) ? m.GetString() : null;
                        _logger.LogWarning("Expo push ticket error: {Message}", msg);
                    }
                }

                if (errors > 0)
                    _logger.LogWarning("Expo push partial failure: {Failure} tickets failed", errors);
            }
            catch (JsonException)
            {
                _logger.LogDebug("Expo push response was not JSON tickets.");
            }
        }

        private sealed class ExpoPushRequest
        {
            public string To { get; set; } = string.Empty;
            public string? Title { get; set; }
            public string? Body { get; set; }
            public Dictionary<string, string>? Data { get; set; }
            public string? Sound { get; set; }
            public string? Priority { get; set; }
            public string? ChannelId { get; set; }
        }
    }
}
