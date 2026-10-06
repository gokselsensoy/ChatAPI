using System;

namespace Application.Features.ChatRooms.DTOs
{
    // Sadece gruplama hesaplaması için geçici olarak kullanılacak
    public class RawReactionDto
    {
        public string Emoji { get; set; }
        public Guid UserId { get; set; }
    }
}