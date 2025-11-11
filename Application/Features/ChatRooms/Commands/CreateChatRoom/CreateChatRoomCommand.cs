using Application.Abstractions.Messaging;
using Domain.Enums;
using System.Text.Json.Serialization;

namespace Application.Features.ChatRooms.Commands.CreateChatRoom
{
    public class CreateChatRoomCommand : ICommand<Guid>
    {
        public string Name { get; set; }
        public RoomType RoomType { get; set; }

        [JsonIgnore] // Bu ID'ler token'dan ve route'dan gelecek
        public Guid BranchId { get; set; }
    }
}
