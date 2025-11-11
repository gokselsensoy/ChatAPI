using Domain.Enums;

namespace Application.Features.ChatRooms.DTOs
{
    public class ChatRoomDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string RoomType { get; set; }
        public Guid BranchId { get; set; }
        public int MemberCount { get; set; }
    }
}
