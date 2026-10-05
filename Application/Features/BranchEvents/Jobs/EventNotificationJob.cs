using Application.Abstractions.Services;
using Domain.Repositories;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.BranchEvents.Jobs
{
    public class EventNotificationJob : IEventNotificationJob
    {
        private readonly IBranchEventRepository _eventRepository;
        private readonly IUserDeviceTokenRepository _deviceTokenRepository;
        private readonly IPushNotificationService _pushNotificationService;

        public EventNotificationJob(
            IBranchEventRepository eventRepository,
            IUserDeviceTokenRepository deviceTokenRepository,
            IPushNotificationService pushNotificationService)
        {
            _eventRepository = eventRepository;
            _deviceTokenRepository = deviceTokenRepository;
            _pushNotificationService = pushNotificationService;
        }

        // Hangfire tam saati geldiğinde bu metodu parametrelerle çağıracak
        public async Task SendReminderAsync(Guid userId, Guid eventId)
        {
            // 1. Etkinlik bilgilerini getir (Sadece başlığı için çekiyoruz)
            // CancellationToken.None veriyoruz çünkü bu işlem arka planda bağımsız çalışıyor
            var branchEvent = await _eventRepository.GetByIdAsync(eventId, CancellationToken.None);

            // Eğer şube sahibi etkinliği o arada tamamen silmişse işlemi sessizce bitir
            if (branchEvent == null) return;

            // 2. Kullanıcının güncel cihaz tokenlarını getir
            var deviceTokens = await _deviceTokenRepository.GetByUserIdAsync(userId, CancellationToken.None);

            // Kullanıcı uygulamayı silmiş veya token'ı yoksa işlemi bitir
            if (deviceTokens == null || !deviceTokens.Any()) return;

            // 3. Bildirim içeriğini hazırla
            string title = "Etkinlik Başlamak Üzere! ⏰"; // Buradaki metinler değişebilir
            string body = $"'{branchEvent.Title}' etkinliği 1 saat sonra başlıyor. Yerin hazır mı?";

            // (Opsiyonel) Mobilde bildirime tıklanınca doğrudan etkinliğin açılması için data
            var dataPayload = new { EventId = branchEvent.Id, Route = "BranchEventsScreen" };

            // 4. Kullanıcının tüm cihazlarına (Örn: Hem iPhone hem iPad kullanıyorsa) bildirimi at
            foreach (var device in deviceTokens)
            {
                // Not: SendNotificationAsync metodunun tam parametre isimleri sendeki arayüze 
                // (IPushNotificationService) göre ufak farklılıklar gösterebilir, orayı kendine göre ayarlarsın.
                await _pushNotificationService.SendNotificationAsync(
                    device.Token,
                    title,
                    body,
                    dataPayload);
            }
        }
    }
}