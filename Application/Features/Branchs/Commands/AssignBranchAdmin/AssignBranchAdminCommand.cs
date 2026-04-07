using MediatR;

namespace Application.Features.Branchs.Commands.AssignBranchAdmin
{
    public class AssignBranchAdminCommand : IRequest<bool>
    {
        public Guid ActingUserId { get; set; }
        public Guid BranchId { get; set; }
        public Guid UserId { get; set; }
    }
}
