using MediatR;

namespace Application.Features.Branchs.Commands.RemoveBranchAdmin
{
    public class RemoveBranchAdminCommand : IRequest<bool>
    {
        public Guid ActingUserId { get; set; }
        public Guid BranchId { get; set; }
        public Guid UserId { get; set; }
    }
}
