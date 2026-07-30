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
            services.Configure<FirebaseOptions>(configuration.GetSection(FirebaseOptions.SectionName));

            services.AddTransient<IPhotoUploader, GoogleDrivePhotoUploader>();
            services.AddSingleton<IPushNotificationService, FcmPushNotificationService>();

            services.AddHttpClient<GoogleDrivePhotoUploader>(client =>
            {
                // client.BaseAddress = new Uri(configuration["GoogleApiSettings:BaseUrl"]);
            });

            return services;
        }
    }
}
