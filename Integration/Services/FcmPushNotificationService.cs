using Application.Abstractions.Services;
using Application.Options;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Integration.Services
{
    public class FcmPushNotificationService : IPushNotificationService
    {
        private readonly FirebaseOptions _options;
        private readonly ILogger<FcmPushNotificationService> _logger;
        private readonly object _initLock = new();
        private bool _initialized;

        public FcmPushNotificationService(
            IOptions<FirebaseOptions> options,
            ILogger<FcmPushNotificationService> logger)
        {
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
                _logger.LogDebug("FCM disabled; push skipped.");
                return;
            }

            var tokens = deviceTokens.Where(t => !string.IsNullOrWhiteSpace(t)).Distinct().ToList();
            if (tokens.Count == 0)
                return;

            if (!EnsureInitialized())
                return;

            // FCM multicast max 500
            foreach (var batch in tokens.Chunk(500))
            {
                var multicast = new MulticastMessage
                {
                    Tokens = batch.ToList(),
                    Notification = new Notification
                    {
                        Title = message.Title,
                        Body = message.Body
                    },
                    Data = message.Data,
                    Android = new AndroidConfig
                    {
                        Priority = Priority.High
                    },
                    Apns = new ApnsConfig
                    {
                        Aps = new Aps
                        {
                            Sound = "default",
                            ContentAvailable = true
                        }
                    }
                };

                try
                {
                    var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(multicast, cancellationToken);
                    if (response.FailureCount > 0)
                    {
                        _logger.LogWarning(
                            "FCM partial failure: {Success} ok, {Failure} failed",
                            response.SuccessCount,
                            response.FailureCount);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "FCM send failed");
                }
            }
        }

        private bool EnsureInitialized()
        {
            if (_initialized && FirebaseApp.DefaultInstance != null)
                return true;

            lock (_initLock)
            {
                if (_initialized && FirebaseApp.DefaultInstance != null)
                    return true;

                if (string.IsNullOrWhiteSpace(_options.CredentialsPath))
                {
                    _logger.LogWarning("Firebase CredentialsPath boş; push atlanıyor.");
                    return false;
                }

                var path = ResolveCredentialsPath(_options.CredentialsPath);
                if (path is null)
                {
                    _logger.LogWarning(
                        "Firebase credentials bulunamadı: {CredentialsPath} (BaseDirectory={BaseDirectory}, Cwd={Cwd})",
                        _options.CredentialsPath,
                        AppContext.BaseDirectory,
                        Directory.GetCurrentDirectory());
                    return false;
                }

                try
                {
                    if (FirebaseApp.DefaultInstance == null)
                    {
                        FirebaseApp.Create(new AppOptions
                        {
                            Credential = GoogleCredential.FromFile(path)
                        });
                    }

                    _initialized = true;
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "FirebaseApp init failed");
                    return false;
                }
            }
        }

        private static string? ResolveCredentialsPath(string credentialsPath)
        {
            if (Path.IsPathRooted(credentialsPath) && File.Exists(credentialsPath))
                return credentialsPath;

            var candidates = new[]
            {
                Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, credentialsPath)),
                Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), credentialsPath)),
                Path.GetFullPath(credentialsPath)
            };

            return candidates.FirstOrDefault(File.Exists);
        }
    }
}
