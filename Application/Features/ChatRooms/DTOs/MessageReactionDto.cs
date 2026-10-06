namespace Application.Features.ChatRooms.DTOs
{
    public class MessageReactionDto
    {
        public string Emoji { get; set; }
        public int Count { get; set; }
        public bool HasReacted { get; set; }
    }
}