using Application.Abstractions.Messaging;
using Application.Features.Branchs.DTOs;
using Application.Shared.Pagination;
using MediatR;

namespace Application.Features.Branchs.Queries.GetBranchesByBrandId
{
    public class GetBranchesByBrandIdQuery : PaginatedRequest, IRequest<PaginatedResponse<BranchDto>>
    {
        public Guid BrandId { get; set; }
    }
}
