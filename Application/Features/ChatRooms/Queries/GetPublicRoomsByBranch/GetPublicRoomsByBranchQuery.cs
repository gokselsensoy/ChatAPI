using Application.Abstractions.Messaging;
using Application.Features.ChatRooms.DTOs;

namespace Application.Features.ChatRooms.Queries.GetPublicRoomsByBranch
{
    public class GetPublicRoomsByBranchQuery : ICachableQuery<List<ChatRoomDto>>
    {
        public Guid BranchId { get; set; }

        public string CacheKey => $"branch:{BranchId}:public_rooms";
        public TimeSpan CacheDuration => TimeSpan.FromMinutes(10);
    }
}
