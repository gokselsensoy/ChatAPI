using Application.Abstractions.QueryRepositories; // Sende IBranchEventQueryRepository oluşturulduysa bunu kullan
using Application.Features.BranchEvents.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.BranchEvents.Queries.GetBranchEvents
{
    public class GetBranchEventsQueryHandler : IRequestHandler<GetBranchEventsQuery, List<BranchEventDto>>
    {
        // Okuma repository'si. (Bunu yazdığını ve içindeki Include(e => e.Participants) yaptığını varsayıyorum)
        private readonly IBranchEventQueryRepository _repository;

        public GetBranchEventsQueryHandler(IBranchEventQueryRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<BranchEventDto>> Handle(GetBranchEventsQuery request, CancellationToken cancellationToken)
        {
            // Şubedeki etkinlikleri participants (katılımcılar) ile birlikte getir
            var events = await _repository.GetEventsByBranchIdAsync(request.BranchId, cancellationToken);

            var dtos = new List<BranchEventDto>();
            var now = DateTime.UtcNow; // UTC kullanmak saat dilimi hatalarını önler

            foreach (var evt in events)
            {
                var dto = new BranchEventDto
                {
                    Id = evt.Id,
                    Title = evt.Title,
                    Description = evt.Description,
                    IsHighlighted = evt.IsHighlighted,
                    ImageUrl = evt.ImageUrl,
                    // Sadece fiilen katılanları say
                    Participants = evt.Participants.Count(p => p.IsAttending)
                };

                // STATÜ HESAPLAMASI (Live, Upcoming, Ended)
                if (now < evt.StartDate)
                {
                    dto.Status = "upcoming";
                    dto.TimeLabel = evt.StartDate.ToString("dd MMM HH:mm");
                }
                else if (now >= evt.StartDate && now <= evt.EndDate)
                {
                    dto.Status = "live";
                    dto.TimeLabel = "Şu An Canlı"; // Mobildeki kırmızı badge için
                }
                else
                {
                    dto.Status = "ended";
                    dto.TimeLabel = "Sona Erdi";
                }

                // O ANKİ KULLANICININ DURUMU
                var userRecord = evt.Participants.FirstOrDefault(p => p.UserId == request.UserId);
                if (userRecord != null)
                {
                    dto.CurrentUserHasJoined = userRecord.IsAttending;
                    dto.CurrentUserHasReminder = userRecord.IsReminderSet;
                }
                else
                {
                    dto.CurrentUserHasJoined = false;
                    dto.CurrentUserHasReminder = false;
                }

                dtos.Add(dto);
            }

            return dtos;
        }
    }
}