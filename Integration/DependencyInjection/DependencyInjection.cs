using Application.Abstractions.Services;
using Application.Options;
using Integration.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Integration.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddIntegrationServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<ExpoPushOptions>(configuration.GetSection(ExpoPushOptions.SectionName));

            services.AddTransient<IPhotoUploader, GoogleDrivePhotoUploader>();
            services.AddHttpClient<IPushNotificationService, ExpoPushNotificationService>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            services.AddHttpClient<GoogleDrivePhotoUploader>(client =>
            {
                // client.BaseAddress = new Uri(configuration["GoogleApiSettings:BaseUrl"]);
            });

            return services;
        }
    }
}
