namespace Application.Features.ChatRooms.DTOs
{
    public class ChatRoomDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string RoomType { get; set; } = string.Empty;
        public Guid BranchId { get; set; }
        public int MemberCount { get; set; }

        public string? LastMessagePreview { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public Guid? LastMessageSenderUserId { get; set; }
        public bool HasNew { get; set; }
        public int UnreadCount { get; set; }
    }
}
