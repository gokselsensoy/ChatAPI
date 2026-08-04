namespace Application.Features.ChatRooms.DTOs
{
    public class ChatRoomMemberDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? FileId { get; set; }
        public bool IsOnline { get; set; }
        public DateTime? LastSeenAt { get; set; }
        public DateTime JoinedAt { get; set; }
        public bool IsMe { get; set; }
    }
}
