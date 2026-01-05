using Infrastructure.Persistence.Context;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace WebApi.Services
{
    // BackgroundService kullanıyoruz ki uygulama açılışını kilitlemesin
    public class OpenIddictWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OpenIddictWorker> _logger;

        public OpenIddictWorker(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<OpenIddictWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Uygulamanın tamamen ayağa kalkması için opsiyonel bekleme
            await Task.Delay(3000, stoppingToken);

            try
            {
                await using var scope = _serviceProvider.CreateAsyncScope();

                // 1. DB Kontrolü
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                if (!await context.Database.CanConnectAsync(stoppingToken))
                {
                    _logger.LogError("Veritabanına ulaşılamadı. Seeding iptal.");
                    return;
                }

                var appManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
                var scopeManager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();

                // ==========================================
                // ADIM 1: SCOPE'LARI OLUŞTUR (ÖNCELİK BURADA)
                // ==========================================

                if (await scopeManager.FindByNameAsync("chatapp_api", stoppingToken) is null)
                {
                    await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
                    {
                        Name = "chatapp_api",
                        DisplayName = "ChatApp API Access",
                        Resources = { "chatapp_api_resource_server" }
                    }, stoppingToken);
                    _logger.LogInformation("Scope 'chatapp_api_api' oluşturuldu.");
                }

                if (await scopeManager.FindByNameAsync("offline_access", stoppingToken) is null)
                {
                    await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
                    {
                        Name = "offline_access",
                        DisplayName = "Offline Access",
                        Description = "Refresh token yetkisi."
                    }, stoppingToken);
                }

                // ==========================================
                // ADIM 2: CLIENT'LARI OLUŞTUR (SCOPE ARTIK HAZIR)
                // ==========================================

                // 2.1 ChatApp API
                var confidentialClientId = "chatapp_api_service";
                if (await appManager.FindByClientIdAsync(confidentialClientId, stoppingToken) is null)
                {
                    var clientSecret = _configuration["Secrets:ClientSecret"];
                    if (!string.IsNullOrEmpty(clientSecret))
                    {
                        await appManager.CreateAsync(new OpenIddictApplicationDescriptor
                        {
                            ClientId = confidentialClientId,
                            ClientSecret = clientSecret,
                            DisplayName = "ChatApp API Service",
                            Permissions =
                            {
                                Permissions.Endpoints.Token,
                                Permissions.GrantTypes.ClientCredentials,
                                Permissions.Prefixes.Scope + "chatapp_api" // Scope artık var, güvenle ekle
                            }
                        }, stoppingToken);
                    }
                }

                // 2.2 Mobile Client
                var mobileClientId = "chat_mobile";
                if (await appManager.FindByClientIdAsync(mobileClientId, stoppingToken) is null)
                {
                    await appManager.CreateAsync(new OpenIddictApplicationDescriptor
                    {
                        ClientId = mobileClientId,
                        DisplayName = "Chat Mobile App",
                        Permissions =
                        {
                            Permissions.Endpoints.Token,
                            Permissions.Endpoints.Revocation,
                            Permissions.GrantTypes.Password,
                            Permissions.GrantTypes.RefreshToken,
                            Permissions.Scopes.Email,
                            Permissions.Scopes.Profile,
                            Permissions.Scopes.Roles,
                            Permissions.Prefixes.Scope + "chatapp_api", // Scope var
                            Permissions.Prefixes.Scope + "offline_access",
                            Permissions.Prefixes.GrantType + "profile_exchange"
                        }
                    }, stoppingToken);
                }

                var webClientId = "chat_web";
                if (await appManager.FindByClientIdAsync(webClientId, stoppingToken) is null)
                {
                    await appManager.CreateAsync(new OpenIddictApplicationDescriptor
                    {
                        ClientId = webClientId,
                        DisplayName = "Chat Web App",
                        Permissions =
                    {
                        Permissions.Endpoints.Token,
                        Permissions.Endpoints.Revocation,
                        Permissions.GrantTypes.Password,
                        Permissions.GrantTypes.RefreshToken,
                        Permissions.Scopes.Email,
                        Permissions.Scopes.Profile,
                        Permissions.Scopes.Roles,
                        Permissions.Prefixes.Scope + "chatapp_api",
                        Permissions.Prefixes.Scope + "offline_access",
                        Permissions.Prefixes.GrantType + "profile_exchange"
                    }
                    }, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OpenIddict Seeding sırasında hata oluştu.");
            }
        }
    }
}
