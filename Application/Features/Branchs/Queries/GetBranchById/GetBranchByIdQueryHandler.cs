using Application.Abstractions.QueryRepositories;
using Application.Exceptions;
using Application.Features.Branchs.DTOs;
using Domain.Entities;
using MediatR;

namespace Application.Features.Branchs.Queries.GetBranchById
{
    public class GetBranchByIdQueryHandler : IRequestHandler<GetBranchByIdQuery, BranchDto>
    {
        private readonly IBranchQueryRepository _branchQueryRepository;

        public GetBranchByIdQueryHandler(IBranchQueryRepository branchQueryRepository)
        {
            _branchQueryRepository = branchQueryRepository;
        }

        public async Task<BranchDto> Handle(GetBranchByIdQuery request, CancellationToken cancellationToken)
        {
            var branch = await _branchQueryRepository.GetByIdAsync(request.BranchId, cancellationToken);
            if (branch == null)
            {
                throw new NotFoundException(nameof(Branch), request.BranchId);
            }
            return branch;
        }
    }
}
