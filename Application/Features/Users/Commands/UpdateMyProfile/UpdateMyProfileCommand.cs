using Application.Abstractions.Messaging;

namespace Application.Features.Users.Commands.UpdateMyProfile
{
    public class UpdateMyProfileCommand : ICommand
    {
        public Guid UserId { get; set; }
        public Guid IdentityId { get; set; }

        // Güncellenecek alanlar
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? FileId { get; set; }
    }
}
