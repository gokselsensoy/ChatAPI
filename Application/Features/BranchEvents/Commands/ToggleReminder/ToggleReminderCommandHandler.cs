using Application.Features.BranchEvents.Jobs;
using Domain.Repositories;
using Domain.SeedWork;
using Hangfire;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Exceptions;

namespace Application.Features.BranchEvents.Commands.ToggleReminder
{
    public class ToggleEventReminderCommandHandler : IRequestHandler<ToggleEventReminderCommand, bool>
    {
        private readonly IBranchEventRepository _eventRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBackgroundJobClient _backgroundJobClient; // Hangfire servisi

        public ToggleEventReminderCommandHandler(
            IBranchEventRepository eventRepository, 
            IUnitOfWork unitOfWork,
            IBackgroundJobClient backgroundJobClient)
        {
            _eventRepository = eventRepository;
            _unitOfWork = unitOfWork;
            _backgroundJobClient = backgroundJobClient;
        }

        public async Task<bool> Handle(ToggleEventReminderCommand request, CancellationToken cancellationToken)
        {
            var branchEvent = await _eventRepository.GetByIdWithParticipantsAsync(request.EventId, cancellationToken);
            if (branchEvent == null) throw new NotFoundException("Etkinlik bulunamadı.");

            var participant = branchEvent.Participants.FirstOrDefault(p => p.UserId == request.UserId);
            
            // Güvenlik Kuralı: Katılmadan hatırlatıcı kurulamaz.
            if (participant == null || !participant.IsAttending)
                throw new Exception("Hatırlatıcı kurabilmek için önce etkinliğe katılmalısınız.");

            // Geçmiş bir etkinlik için hatırlatıcı kurulamaz
            if (branchEvent.StartDate <= DateTime.UtcNow)
                throw new Exception("Başlamış veya bitmiş bir etkinlik için hatırlatıcı kurulamaz.");

            if (!participant.IsReminderSet)
            {
                var triggerTime = branchEvent.StartDate.AddHours(-1); // 1 saat önce

                // Eğer etkinliğe 1 saatten daha az bir süre kalmışsa, bildirimi 10 saniye sonra hemen gönder.(Buranın nasıl çalışacağından emin değilim eğer spam durumu olursa burayı düzelt.)
                if (triggerTime <= DateTime.UtcNow)
                    triggerTime = DateTime.UtcNow.AddSeconds(10);

                // Hangfire'a görevi veriyoruz (Zamanı geldiğinde IEventNotificationJob çalışacak)
                var jobId = _backgroundJobClient.Schedule<IEventNotificationJob>(
                    job => job.SendReminderAsync(request.UserId, request.EventId),
                    triggerTime);

                participant.SetReminder(jobId);
            }
            else
            {
                // 2. DURUM: HATIRLATICIYI İPTAL ETME
                if (!string.IsNullOrEmpty(participant.HangfireJobId))
                {
                    _backgroundJobClient.Delete(participant.HangfireJobId);
                }
                participant.CancelReminder();
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return participant.IsReminderSet;
        }
    }
}