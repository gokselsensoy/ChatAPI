using System;
using System.Collections.Generic;

namespace Application.Features.BranchEvents.DTOs
{
    public class BranchEventDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        // Dinamik hesaplanacak alanlar
        public string Status { get; set; } // 'live', 'upcoming', 'ended'
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string TimeLabel { get; set; }
        public int Participants { get; set; }

        // Görünüm ve meta alanları
        public bool IsHighlighted { get; set; }
        public string? ImageUrl { get; set; }
        public string? Category { get; set; }
        public string? SessionLabel { get; set; }
        public string? HostName { get; set; }
        public string? PriceLabel { get; set; }
        public int? SeatsLeft { get; set; }
        public List<string> ParticipantInitials { get; set; } = new List<string>();

        // Kullanıcıya özel durumlar (Mobil buraya bakıp hangi butonu çizeceğini bilecek)
        public bool CurrentUserHasJoined { get; set; }
        public bool CurrentUserHasReminder { get; set; }
    }
}