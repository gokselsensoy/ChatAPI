using Application.Abstractions.Messaging;
using Application.Features.Branchs.DTOs;
using Application.Shared.Pagination;
using MediatR;

namespace Application.Features.Branchs.Queries.GetBranchById
{
    public class GetBranchByIdQuery : IRequest<BranchDto>
    {
        public Guid BranchId { get; set; }
    }
}
