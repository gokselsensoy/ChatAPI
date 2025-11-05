using Application.Abstractions.QueryRepositories;
using Application.Exceptions;
using Application.Features.Users.DTOs;
using MediatR;

namespace Application.Features.Users.Queries.GetMyProfile
{
    public class GetMyProfileQueryHandler : IRequestHandler<GetMyProfileQuery, UserDto>
    {
        private readonly IUserQueryRepository _userQueryRepository;

        public GetMyProfileQueryHandler(IUserQueryRepository userQueryRepository)
        {
            _userQueryRepository = userQueryRepository;
        }

        public async Task<UserDto> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
        {
            var user = await _userQueryRepository.GetByIdentityIdAsync(request.IdentityId, cancellationToken);
            if (user == null)
            {
                // Token geçerli ama profil (Users tablosunda) yok.
                throw new NotFoundException($"User profile not found for IdentityId: {request.IdentityId}");
            }
            return user;
        }
    }
}
