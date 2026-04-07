using Application.Features.Branchs.DTOs;
using MediatR;

namespace Application.Features.Branchs.Queries.GetBranchAdmins
{
    public class GetBranchAdminsQuery : IRequest<List<BranchAdminListItemDto>>
    {
        public Guid ActingUserId { get; set; }
        public Guid BranchId { get; set; }
    }
}
