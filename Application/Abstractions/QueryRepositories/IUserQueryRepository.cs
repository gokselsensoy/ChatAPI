using Application.Features.Users.DTOs;

namespace Application.Abstractions.QueryRepositories
{
    public interface IUserQueryRepository
    {
        Task<UserDto?> GetByIdentityIdAsync(Guid identityId, CancellationToken cancellationToken = default);
    }
}
