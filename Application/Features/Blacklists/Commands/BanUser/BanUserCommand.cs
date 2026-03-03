using MediatR;

namespace Application.Features.Blacklists.Commands.BanUser
{
    public class BanUserCommand : IRequest<bool>
    {
        public Guid UserId { get; set; }
        public Guid BranchId { get; set; }
        public string Reason { get; set; }
        public DateTime? FinishTime { get; set; }
    }
}
