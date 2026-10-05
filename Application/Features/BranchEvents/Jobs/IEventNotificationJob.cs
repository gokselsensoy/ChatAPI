using System;
using System.Threading.Tasks;

namespace Application.Features.BranchEvents.Jobs
{
    // Hangfire'ın tetikleyeceği sınıfın arayüzü (İçini Faz 4'te dolduracağız)
    public interface IEventNotificationJob
    {
        Task SendReminderAsync(Guid userId, Guid eventId);
    }
}