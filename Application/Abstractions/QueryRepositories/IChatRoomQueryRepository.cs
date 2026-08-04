using Application.Features.ChatRooms.DTOs;
using Application.Shared.Pagination;
using Domain.Enums;

namespace Application.Abstractions.QueryRepositories
{
    public interface IChatRoomQueryRepository
    {
        Task<List<ChatRoomDto>> GetPublicRoomsByBranchIdAsync(
            Guid branchId,
            Guid currentUserId,
            CancellationToken cancellationToken = default);

        Task<List<ChatRoomDto>> GetPrivateInboxAsync(
            Guid userId,
            CancellationToken cancellationToken = default);

        Task<List<ChatRoomDto>> GetGroupInboxAsync(
            Guid userId,
            CancellationToken cancellationToken = default);

        /// <summary>Odadaki üye userId listesi (presence sayımı için).</summary>
        Task<Dictionary<Guid, List<Guid>>> GetMemberUserIdsByRoomIdsAsync(
            IEnumerable<Guid> roomIds,
            CancellationToken cancellationToken = default);

        Task<List<ChatRoomMemberDto>> GetMembersForRoomAsync(
            Guid roomId,
            CancellationToken cancellationToken = default);

        Task<PaginatedResponse<ChatRoomMessageDto>> GetMessagesForRoomAsync(
            Guid roomId,
            Guid branchId,
            RoomType roomType,
            PaginatedRequest pagination,
            Guid currentUserId,
            CancellationToken cancellationToken = default);
    }
}
