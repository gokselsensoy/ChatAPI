using MediatR;

namespace Application.Features.Blacklists.Commands.LiftBan
{
    public class LiftBanCommand : IRequest<bool>
    {
        public Guid BranchId { get; set; }
        public Guid UserId { get; set; }
    }
}
