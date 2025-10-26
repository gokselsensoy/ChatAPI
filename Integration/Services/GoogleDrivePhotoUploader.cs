using Application.Abstractions.Services;
using Microsoft.Extensions.Logging;

namespace Integration.Services
{
    public class GoogleDrivePhotoUploader : IPhotoUploader
    {
        private readonly ILogger<GoogleDrivePhotoUploader> _logger;
        private readonly INotificationService _notificationService;

        public GoogleDrivePhotoUploader(
            ILogger<GoogleDrivePhotoUploader> logger,
            INotificationService notificationService)
        {
            _logger = logger;
            _notificationService = notificationService;
        }

        public async Task UploadOrderPhotoAsync(Guid orderId, string photoBase64, Guid customerId)
        {
            try
            {
                _logger.LogInformation("Google Drive'a yükleme başlıyor. OrderId: {OrderId}", orderId);

                await Task.Delay(5000); // 5 saniye beklet

                // Eğer GDrive API'si hata dönerse (simülasyon):
                bool uploadError = false; // Bunu true yaparsan hata senaryosunu test edersin
                if (uploadError)
                {
                    throw new Exception("Google Drive API yanıt vermedi.");
                }

                _logger.LogInformation("Google Drive'a yükleme başarılı. OrderId: {OrderId}", orderId);

                // İsteğe bağlı: Başarı bildirimi de gönderebilirsin
                await _notificationService.SendNotificationToUserAsync(
                    customerId.ToString(),
                    "PhotoUploadSuccess",
                    new { OrderId = orderId, Message = "Sipariş fotoğrafınız başarıyla yüklendi." }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Google Drive yüklemesi HATA ALDI. OrderId: {OrderId}", orderId);

                // Hangfire bu metodu yeniden deneyecek.
                // Eğer X denemeden sonra hala başarısızsa,
                // Kullanıcıya SignalR ile haber ver:
                await _notificationService.SendNotificationToUserAsync(
                    customerId.ToString(),
                    "PhotoUploadFailed", // İstemcinin dinleyeceği metot
                    new { OrderId = orderId, Message = $"Sipariş fotoğrafınız yüklenemedi: {ex.Message}" }
                );

                // Hatayı tekrar fırlat ki Hangfire "Failed" olarak işaretlesin ve dashboard'da görünsün.
                throw;
            }
        }
    }
}
