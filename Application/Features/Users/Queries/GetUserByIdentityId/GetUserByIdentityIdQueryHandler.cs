using Application.Abstractions.QueryRepositories;
using Application.Features.Users.DTOs;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Users.Queries.GetUserByIdentityId
{
    public class GetUserByIdentityIdQueryHandler : IRequestHandler<GetUserByIdentityIdQuery, UserDto?>
    {
        private readonly IUserQueryRepository _userQueryRepository;

        public GetUserByIdentityIdQueryHandler(IUserQueryRepository userQueryRepository)
        {
            _userQueryRepository = userQueryRepository;
        }

        public async Task<UserDto?> Handle(GetUserByIdentityIdQuery request, CancellationToken cancellationToken)
        {
            return await _userQueryRepository.GetByIdentityIdAsync(request.IdentityId, cancellationToken);
        }
    }
}