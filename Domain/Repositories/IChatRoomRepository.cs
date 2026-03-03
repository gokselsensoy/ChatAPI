using Domain.Entities;
using Domain.SeedWork;

namespace Domain.Repositories
{
    public interface IChatRoomRepository : IRepository<ChatRoom>
    {
        Task<ChatRoom?> GetByIdWithUsersAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ChatRoom?> GetByIdWithMessagesAndUsersAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<ChatRoom>> GetRoomsByUserAndBranchAsync(Guid userId, Guid branchId, CancellationToken cancellationToken = default);
    }
}
