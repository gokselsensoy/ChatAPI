using Application.Features.ChatRooms.DTOs;
using Application.Shared.Pagination;

namespace Application.Abstractions.QueryRepositories
{
    public interface IChatRoomQueryRepository
    {
        Task<List<ChatRoomDto>> GetPublicRoomsByBranchIdAsync(Guid branchId, CancellationToken cancellationToken = default);

        Task<PaginatedResponse<ChatRoomMessageDto>> GetMessagesForRoomAsync(
        Guid roomId,
        PaginatedRequest pagination,
        CancellationToken cancellationToken = default);
    }
}
