using System;

namespace Application.Features.ChatRoomInvites.DTOs
{
    public class ChatRoomInviteDto
    {
        public Guid Id { get; set; }
        public Guid ChatRoomId { get; set; }
        public Guid InviterUserId { get; set; }
        public string InviterUserName { get; set; } = string.Empty;
        public string? InviterFileId { get; set; }
        public string TargetRoomType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
