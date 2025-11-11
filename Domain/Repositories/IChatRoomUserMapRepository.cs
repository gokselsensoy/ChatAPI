using Domain.Entities;
using Domain.SeedWork;

namespace Domain.Repositories
{
    public interface IChatRoomUserMapRepository : IRepository<ChatRoomUserMap>
    {
        Task<ChatRoomUserMap?> FindByRoomAndUserAsync(Guid chatRoomId, Guid userId, CancellationToken cancellationToken = default);
    }
}
