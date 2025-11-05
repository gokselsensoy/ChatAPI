using Application.Abstractions.Messaging;
using Domain.Enums;

namespace Application.Features.Users.Commands.CreateUser
{
    public class CreateUserCommand : ICommand<Guid>
    {
        public Guid IdentityId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public UserType UserType { get; set; } = UserType.Customer;
    }
}
