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
        public bool IsMine { get; set; }

        /// <summary>Şubede admin/marka sahibi ise "Admin", aksi halde "Müşteri".</summary>
        public string SenderRole { get; set; } = string.Empty;
    }
}