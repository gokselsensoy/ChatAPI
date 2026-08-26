using Domain.Entities;
using Domain.Enums;
using Domain.SeedWork;

namespace Domain.Repositories
{
    public interface IChatRoomRepository : IRepository<ChatRoom>
    {
        Task<ChatRoom?> GetByIdWithUsersAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ChatRoom?> GetByIdWithMessagesAndUsersAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ChatRoomMessage?> GetMessageByIdAsync(Guid messageId, CancellationToken cancellationToken = default);
        Task<List<ChatRoom>> GetRoomsByUserAndBranchAsync(Guid userId, Guid branchId, CancellationToken cancellationToken = default);
        Task MarkAsReadAsync(Guid roomId, Guid userId, CancellationToken cancellationToken = default);

        Task<ChatRoom?> FindDirectRoomBetweenUsersAsync(
            Guid userId1,
            Guid userId2,
            RoomType roomType,
            CancellationToken cancellationToken = default);

        /// <summary>Private + Group odalarındaki diğer üyeler (presence yayını).</summary>
        Task<List<Guid>> GetSharedRoomPeerUserIdsAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
