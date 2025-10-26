using Application.Features.Branchs.DTOs;
using Application.Shared.Pagination;
using MediatR;

namespace Application.Features.Branchs.Queries.GetBranchesByBrandId
{
    public class GetBranchesByBrandIdQuery : PaginatedRequest, IRequest<PaginatedResponse<BranchDto>>
    {
        // Bu ID, Controller'da route'dan (URL) alınacak
        public Guid BrandId { get; set; }
    }
}
