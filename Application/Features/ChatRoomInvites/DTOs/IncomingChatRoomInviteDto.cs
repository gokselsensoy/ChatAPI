namespace Application.Features.ChatRoomInvites.DTOs
{
    public class IncomingChatRoomInviteDto
    {
        public Guid InviteId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }

        /// <summary>Private = geo'suz 1:1; Group = geo'lu grup.</summary>
        public string TargetRoomType { get; set; } = string.Empty;

        public Guid InviterUserId { get; set; }
        public string InviterUserName { get; set; } = string.Empty;
        public string InviterFirstName { get; set; } = string.Empty;
        public string InviterLastName { get; set; } = string.Empty;
        public string? InviterFileId { get; set; }

        /// <summary>Davetin atıldığı andaki şube (kaynak odanın şubesi).</summary>
        public Guid BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;

        /// <summary>Davetin gönderildiği public / kaynak oda.</summary>
        public Guid SourceChatRoomId { get; set; }
        public string SourceChatRoomName { get; set; } = string.Empty;

        /// <summary>Yalnızca Group davetlerinde dolu. Kaynak oda grupsa oda adı, değilse varsayılan grup adı.</summary>
        public string? GroupName { get; set; }
    }
}
