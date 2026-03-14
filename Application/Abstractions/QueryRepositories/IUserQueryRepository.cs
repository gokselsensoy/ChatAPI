using Application.Features.Users.DTOs;
using Domain.Entities;
using System.Linq.Expressions;

namespace Application.Abstractions.QueryRepositories
{
    public interface IUserQueryRepository
    {
        Task<User?> GetAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken = default);
        Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<UserDto?> GetByIdentityIdAsync(Guid identityId, CancellationToken cancellationToken = default);
        Task<Dictionary<Guid, Guid?>> GetUserBranchMapAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);
    }
}
