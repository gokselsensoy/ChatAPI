namespace Application.Features.ChatRooms.DTOs
{
    public class ChatRoomMessageDto
    {
        public Guid Id { get; set; }
        public Guid ChatRoomId { get; set; }
        public Guid SenderUserId { get; set; }
        public string SenderUserName { get; set; }
        public string Message { get; set; }
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Bu mesajı görüntüleyen kullanıcı mı yazdı. GET ve gönderen SignalR payload'unda
        /// viewer'a göre set edilir; grup yayınına IsMine=false gider.
        /// </summary>
        public bool IsMine { get; set; }

        /// <summary>Şubede admin/marka sahibi ise "Admin", aksi halde "Müşteri".</summary>
        public string SenderRole { get; set; } = string.Empty;

        /// <summary>Yanıtlanan mesajın id'si. Yanıt değilse null.</summary>
        public Guid? ReplyToMessageId { get; set; }

        /// <summary>Alıntılanan mesajın yazarı. Client: replyToSenderUserId == currentUserId.</summary>
        public Guid? ReplyToSenderUserId { get; set; }

        public string? ReplyToSenderUserName { get; set; }

        /// <summary>Alıntılanan mesaj metni (WhatsApp quote preview).</summary>
        public string? ReplyToMessage { get; set; }

        /// <summary>
        /// Alıntılanan mesajı görüntüleyen kullanıcı mı yazdı.
        /// GET /messages içinde current user'a göre doğrudur.
        /// SignalR ReceiveMessage grup yayınında viewer bilinmez; client
        /// replyToSenderUserId == kendi userId ile hesaplamalıdır.
        /// </summary>
        public bool ReplyToIsMine { get; set; }
    }
}
