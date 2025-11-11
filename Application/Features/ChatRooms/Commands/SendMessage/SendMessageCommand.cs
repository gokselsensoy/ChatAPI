using Application.Abstractions.Messaging;
using Application.Features.ChatRooms.DTOs;
using System.Text.Json.Serialization;

namespace Application.Features.ChatRooms.Commands.SendMessage
{
    public class SendMessageCommand : ICommand<ChatRoomMessageDto>
    {
        public string Message { get; set; }
        public Guid RoomId { get; set; }

        [JsonIgnore]
        public Guid SenderUserId { get; set; }
        [JsonIgnore]
        public string SenderUserName { get; set; } // DTO için
    }
}
