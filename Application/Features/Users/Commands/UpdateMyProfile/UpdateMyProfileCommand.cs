using Application.Abstractions.Messaging;
using System.Text.Json.Serialization;

namespace Application.Features.Users.Commands.UpdateMyProfile
{
    public class UpdateMyProfileCommand : ICommand
    {
        [JsonIgnore]
        public Guid UserId { get; set; }
        [JsonIgnore]
        public Guid IdentityId { get; set; }

        // Güncellenecek alanlar
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? FileId { get; set; }
    }
}
