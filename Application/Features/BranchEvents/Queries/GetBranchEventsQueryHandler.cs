using Application.Abstractions.QueryRepositories;
using Application.Features.BranchEvents.DTOs;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.BranchEvents.Queries.GetBranchEvents
{
    public class GetBranchEventsQueryHandler : IRequestHandler<GetBranchEventsQuery, List<BranchEventDto>>
    {
        private static readonly CultureInfo TurkishCulture = CultureInfo.GetCultureInfo("tr-TR");
        private readonly IBranchEventQueryRepository _repository;

        public GetBranchEventsQueryHandler(IBranchEventQueryRepository repository)
        {
            _repository = repository;
        }

        private static string ExtractInitials(User user)
        {
            var fullName = $"{user.FirstName} {user.LastName}".Trim();
            var source = !string.IsNullOrWhiteSpace(fullName) ? fullName : user.UserName?.Trim();

            if (string.IsNullOrWhiteSpace(source))
            {
                return "U";
            }

            var words = source.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 1)
            {
                var word = words[0];
                return word.Substring(0, Math.Min(2, word.Length)).ToUpper(TurkishCulture);
            }

            var firstChar = words[0][0];
            var lastChar = words[^1][0];
            return $"{firstChar}{lastChar}".ToUpper(TurkishCulture);
        }

        public async Task<List<BranchEventDto>> Handle(GetBranchEventsQuery request, CancellationToken cancellationToken)
        {
            // Şubedeki etkinlikleri participants (katılımcılar) ile birlikte getir
            var events = await _repository.GetEventsByBranchIdAsync(request.BranchId, cancellationToken);

            var dtos = new List<BranchEventDto>();
            var now = DateTime.UtcNow; // UTC kullanmak saat dilimi hatalarını önler

            foreach (var evt in events)
            {
                var attendingParticipants = evt.Participants
                    .Where(p => p.IsAttending && !p.IsDeleted)
                    .ToList();

                var participantInitials = attendingParticipants
                    .Where(p => p.User != null)
                    .Select(p => ExtractInitials(p.User!))
                    .Where(initials => !string.IsNullOrWhiteSpace(initials))
                    .Take(3)
                    .ToList();

                var dto = new BranchEventDto
                {
                    Id = evt.Id,
                    Title = evt.Title,
                    Description = evt.Description,
                    IsHighlighted = evt.IsHighlighted,
                    ImageUrl = evt.ImageUrl,
                    StartDate = evt.StartDate,
                    EndDate = evt.EndDate,
                    // Sadece fiilen katılanları say
                    Participants = attendingParticipants.Count,
                    ParticipantInitials = participantInitials
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
                var userRecord = evt.Participants.FirstOrDefault(p => p.UserId == request.UserId && !p.IsDeleted);
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