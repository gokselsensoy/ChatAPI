using Application.Features.Users.DTOs;

namespace Application.Abstractions.QueryRepositories
{
    public interface IUserQueryRepository
    {
        Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<UserDto?> GetByIdentityIdAsync(Guid identityId, CancellationToken cancellationToken = default);
        Task<Dictionary<Guid, Guid?>> GetUserBranchMapAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);
    }
}
