using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstractions.Services;
using Expo.Server.Client;
using Expo.Server.Models;
using Microsoft.Extensions.Configuration;
using Serilog;
using Hangfire;

namespace Infrastructure.Services
{
    public class ExpoPushNotificationService : IPushNotificationService
    {
        private readonly PushApiClient _expoClient;
        private readonly ILogger _logger;

        public ExpoPushNotificationService(IConfiguration configuration)
        {
            _logger = Log.ForContext<ExpoPushNotificationService>();
            
            var accessToken = configuration["ExpoPushSettings:AccessToken"];
            _expoClient = new PushApiClient(); 
            // If the library supports initialization with access token, you should configure it here.
        }

        public Task SendToTokensAsync(IReadOnlyList<string> deviceTokens, PushMessage message, CancellationToken cancellationToken = default)
        {
            if (deviceTokens == null || !deviceTokens.Any())
                return Task.CompletedTask;

            // Arka planda Hangfire üzerinden gönderimi tetiklemek:
            BackgroundJob.Enqueue(() => ExecutePushAsync(deviceTokens.ToList(), message));
            
            return Task.CompletedTask;
        }

        [AutomaticRetry(Attempts = 3)]
        public async Task ExecutePushAsync(List<string> tokens, PushMessage message)
        {
            var pushTicketsReq = new PushTicketRequest()
            {
                PushTo = tokens,
                PushBadgeCount = 1,
                PushTitle = message.Title,
                PushBody = message.Body,
                PushData = message.Data
            };

            try
            {
                var result = await _expoClient.PushSendAsync(pushTicketsReq);
                
                if (result?.PushTicketErrors?.Any() == true)
                {
                    foreach (var error in result.PushTicketErrors)
                    {
                        _logger.Error("Expo Push Error: {ErrorCode} - {ErrorMessage}", error.ErrorCode, error.ErrorMessage);
                    }
                }

                if (result?.PushTicketStatuses?.Any() == true)
                {
                    foreach (var status in result.PushTicketStatuses)
                    {
                        if (status.TicketStatus == "error")
                        {
                            _logger.Error("Expo Push Status Error: {ErrorDetail}", status.TicketMessage);
                        }
                    }
                }
                
                _logger.Information("Push notification sent to {Count} devices.", tokens.Count);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to send push notification via Expo.");
                throw; // Rethrow to let Hangfire retry
            }
        }
    }
}
