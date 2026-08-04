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

        /// <summary>Private 1:1 için karşı taraf. Group/Public'te null olabilir.</summary>
        public Guid? PeerUserId { get; set; }
        public string? PeerUserName { get; set; }
        public string? PeerFileId { get; set; }
        public bool? IsOnline { get; set; }
        public DateTime? LastSeenAt { get; set; }

        /// <summary>Group / Public: o anda hub'a bağlı üye sayısı (LastSeen değil).</summary>
        public int OnlineMemberCount { get; set; }
    }
}
