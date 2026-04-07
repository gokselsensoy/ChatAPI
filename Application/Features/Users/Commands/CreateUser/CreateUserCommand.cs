using Application.Abstractions.Messaging;

namespace Application.Features.Users.Commands.CreateUser
{
    public class CreateUserCommand : ICommand<Guid>
    {
        public Guid IdentityId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
