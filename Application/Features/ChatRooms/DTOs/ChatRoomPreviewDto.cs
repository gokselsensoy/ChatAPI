namespace Application.Features.ChatRooms.DTOs
{
    /// <summary>
    /// Liste ekranları için hafif SignalR önizleme payload'u.
    /// </summary>
    public class ChatRoomPreviewDto
    {
        public Guid RoomId { get; set; }
        public string RoomType { get; set; } = string.Empty;
        public Guid BranchId { get; set; }
        public string? LastMessagePreview { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public Guid? SenderUserId { get; set; }
        public bool HasNew { get; set; } = true;
        public int UnreadCount { get; set; } = 1;
        public Guid? PeerUserId { get; set; }
    }
}
