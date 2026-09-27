using Application.Abstractions.Messaging;
using System.Text.Json.Serialization;

namespace Application.Features.ChatRooms.Commands.ToogleReaction
{
    // Dışarıdan sadece MesssageId ve Emoji bilgisini alıyoruz.
    // Barşı durumunu 'bool' olarak döneceğiz.
    public class ToggleReactionCommand : ICommand<bool>
    {
        public Guid MessageId { get; set; }
        public string Emoji
    }
}
