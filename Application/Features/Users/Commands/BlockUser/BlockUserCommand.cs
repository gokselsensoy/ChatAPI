using MediatR;

namespace Application.Features.Users.Commands.BlockUser
{
    public class BlockUserCommand : IRequest
    {
        public Guid BlockerUserId { get; set; }
        public Guid BlockedUserId { get; set; }
    }
}
