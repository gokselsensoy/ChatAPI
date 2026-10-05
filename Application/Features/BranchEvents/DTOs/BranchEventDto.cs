using System;

namespace Application.Features.BranchEvents.DTOs
{
    public class BranchEventDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        // Dinamik hesaplanacak alanlar
        public string Status { get; set; } // 'live', 'upcoming', 'ended'
        public string TimeLabel { get; set; }
        public int Participants { get; set; }

        // Görünüm alanları
        public bool IsHighlighted { get; set; }
        public string? ImageUrl { get; set; }

        // Kullanıcıya özel durumlar (Mobil buraya bakıp hangi butonu çizeceğini bilecek)
        public bool CurrentUserHasJoined { get; set; }
        public bool CurrentUserHasReminder { get; set; }
    }
}