using Application.Abstractions.Messaging;
using System.Text.Json.Serialization;

namespace Application.Features.ChatRooms.Commands.ToogleReaction
{
    // Dışarıdan sadece MesssageId ve Emoji bilgisini alıyoruz.
    // Başarıı durumunu 'bool' olarak döneceğiz.
    public class ToggleReactionCommand : ICommand<bool>
    {
        public Guid MessageId { get; set; }
        public string Emoji { get; set; }

        [JsonIgnore] //Controller'da JWT Token içinden alınacak, dışarıdan (JSON olarak) göndirlmesine izin vermiyoruz ki başkasının adına işlem yapılmasın
        public Guid UserId {get; set;}
        
    }
}
