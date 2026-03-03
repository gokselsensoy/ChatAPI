using MediatR;

namespace Application.Features.Blacklists.Commands.UpdateBan
{
    public class UpdateBanCommand : IRequest<bool>
    {
        public Guid BranchId { get; set; }
        public Guid UserId { get; set; }
        public DateTime? NewFinishTime { get; set; } // Null ise "Sınırsız" ban olur
    }
}
