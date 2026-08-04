using Application.Abstractions.QueryRepositories;
using Application.Abstractions.Services;
using Application.Features.ChatRooms.DTOs;

namespace Application.Features.ChatRooms.Queries
{
    internal static class PresenceEnrichment
    {
        public static async Task ApplyOnlineMemberCountsAsync(
            List<ChatRoomDto> rooms,
            IChatRoomQueryRepository chatRoomQueryRepository,
            IPresenceService presenceService,
            CancellationToken cancellationToken)
        {
            if (rooms.Count == 0)
                return;

            var membersByRoom = await chatRoomQueryRepository.GetMemberUserIdsByRoomIdsAsync(
                rooms.Select(r => r.Id),
                cancellationToken);

            var allUserIds = membersByRoom.Values.SelectMany(x => x).Distinct().ToList();
            var onlineMap = presenceService.GetOnlineStatus(allUserIds);

            foreach (var room in rooms)
            {
                if (!membersByRoom.TryGetValue(room.Id, out var memberIds))
                {
                    room.OnlineMemberCount = 0;
                    continue;
                }

                room.OnlineMemberCount = memberIds.Count(id =>
                    onlineMap.TryGetValue(id, out var online) && online);
            }
        }
    }
}
