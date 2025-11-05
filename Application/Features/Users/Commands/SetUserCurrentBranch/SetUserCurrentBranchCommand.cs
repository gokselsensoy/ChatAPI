using Application.Abstractions.Messaging;

namespace Application.Features.Users.Commands.SetUserCurrentBranch
{
    public class SetUserCurrentBranchCommand : ICommand
    {
        public Guid UserId { get; set; }
        public Guid? NewBranchId { get; set; }
    }
}
