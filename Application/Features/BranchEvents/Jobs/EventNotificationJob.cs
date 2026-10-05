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
            // 1. Etkinlik bilgilerini getir
            var branchEvent = await _eventRepository.GetByIdAsync(eventId, CancellationToken.None);
            if (branchEvent == null) return;

            // 2. Kullanıcının cihaz token nesnesini getir
            var userDevice = await _deviceTokenRepository.GetByIdAsync(userId, CancellationToken.None);
            if (userDevice == null || string.IsNullOrEmpty(userDevice.Token)) return;

            // 3. Bildirim içeriğini hazırla
            string title = "Etkinlik Başlamak Üzere! ⏰";
            string body = $"'{branchEvent.Title}' etkinliği 1 saat sonra başlıyor. Yerin hazır mı?";

            // 4. Eski dataPayload satırını tamamen sildik, sadece Dictionary olanı kullanıyoruz:
            var dataPayload = new Dictionary<string, string>
            {
                { "EventId", branchEvent.Id.ToString() },
                { "Route", "BranchEventsScreen" }
            };

            // 5. Verileri PushMessage nesnesine dönüştür
            var pushMessage = new PushMessage
            {
                Title = title,
                Body = body,
                Data = dataPayload // Sınıfınızda adı farklıysa (örn: CustomData) burayı güncelleyin
            };

            // 6. Token'ı bir listeye sararak metodu çağır
            await _pushNotificationService.SendToTokensAsync(
                new List<string> { userDevice.Token },
                pushMessage
            );
        }
    }
}