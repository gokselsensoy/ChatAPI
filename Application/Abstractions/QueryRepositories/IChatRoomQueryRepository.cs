using Application.Features.ChatRooms.DTOs;
using Application.Shared.Pagination;
using Domain.Enums;

namespace Application.Abstractions.QueryRepositories
{
    public interface IChatRoomQueryRepository
    {
        Task<List<ChatRoomDto>> GetPublicRoomsByBranchIdAsync(Guid branchId, CancellationToken cancellationToken = default);

        Task<PaginatedResponse<ChatRoomMessageDto>> GetMessagesForRoomAsync(
        Guid roomId,
        RoomType roomType,
        PaginatedRequest pagination,
        Guid currentUserId,
        CancellationToken cancellationToken = default);
    }
}
