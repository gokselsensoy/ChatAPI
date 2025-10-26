using Application.Abstractions.Services;
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
            // GoogleDrive servisini (ve gelecekteki diğerlerini) kaydet
            services.AddTransient<IPhotoUploader, GoogleDrivePhotoUploader>();

            // Buraya Stripe servisi, Email servisi (SendGrid) vb. eklenecek
            // services.AddTransient<IEmailService, SendGridEmailService>();

            // HttpClientFactory ayarlarını da burada yapabilirsin
            services.AddHttpClient<GoogleDrivePhotoUploader>(client =>
            {
                // client.BaseAddress = new Uri(configuration["GoogleApiSettings:BaseUrl"]);
            });

            return services;
        }
    }
}
