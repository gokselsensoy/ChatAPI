using Application.Abstractions.QueryRepositories;
using Application.Abstractions.Services;
using Application.Features.Users.DTOs;
using Domain.Entities;
using Domain.Repositories;
using Domain.SeedWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace WebApi.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IUserQueryRepository _userQueryRepository;
        private readonly IUserRepository _userRepository;
        private readonly IChatRoomRepository _chatRoomRepository;
        private readonly IUserLocationQueryRepository _userLocationQueryRepository;
        private readonly IBranchQueryRepository _branchQueryRepository;
        private readonly IPresenceService _presenceService;
        private readonly INotificationService _notificationService;
        private readonly IUnitOfWork _unitOfWork;

        public ChatHub(
            IUserQueryRepository userQueryRepository,
            IUserRepository userRepository,
            IChatRoomRepository chatRoomRepository,
            IUserLocationQueryRepository userLocationQueryRepository,
            IBranchQueryRepository branchQueryRepository,
            IPresenceService presenceService,
            INotificationService notificationService,
            IUnitOfWork unitOfWork)
        {
            _userQueryRepository = userQueryRepository;
            _userRepository = userRepository;
            _chatRoomRepository = chatRoomRepository;
            _userLocationQueryRepository = userLocationQueryRepository;
            _branchQueryRepository = branchQueryRepository;
            _presenceService = presenceService;
            _notificationService = notificationService;
            _unitOfWork = unitOfWork;
        }

        public override async Task OnConnectedAsync()
        {
            var identityId = GetIdentityIdString();
            if (!string.IsNullOrEmpty(identityId))
                await Groups.AddToGroupAsync(Context.ConnectionId, identityId);

            var currentUser = await GetCurrentUserAsync();
            if (currentUser != null)
            {
                var becameOnline = _presenceService.SetOnline(currentUser.Id);
                if (becameOnline)
                    await NotifySharedPeersAsync(currentUser.Id, isOnline: true, lastSeenAt: null);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser != null)
            {
                var becameOffline = _presenceService.SetOffline(currentUser.Id);
                if (becameOffline)
                {
                    var user = await _userRepository.GetByIdAsync(currentUser.Id, Context.ConnectionAborted);
                    DateTime? lastSeen = DateTime.UtcNow;
                    if (user != null)
                    {
                        user.TouchLastSeen();
                        await _unitOfWork.SaveChangesAsync(Context.ConnectionAborted);
                        lastSeen = user.LastSeenAt;
                    }

                    await NotifySharedPeersAsync(currentUser.Id, isOnline: false, lastSeenAt: lastSeen);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// LastSeen dışında gerçek online: hub bağlantısı. İstenen userId listesi için durum döner.
        /// </summary>
        public Task<object[]> QueryPresence(string[] userIds)
        {
            var ids = (userIds ?? Array.Empty<string>())
                .Select(s => Guid.TryParse(s, out var g) ? g : Guid.Empty)
                .Where(g => g != Guid.Empty)
                .Distinct()
                .ToList();

            var map = _presenceService.GetOnlineStatus(ids);
            var result = ids
                .Select(id => (object)new { UserId = id, IsOnline = map.TryGetValue(id, out var o) && o })
                .ToArray();

            return Task.FromResult(result);
        }

        public async Task JoinBranchChannel(string branchId)
        {
            if (!Guid.TryParse(branchId, out var branchGuid))
                throw new HubException("Geçersiz branchId.");

            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
                throw new HubException("Kullanıcı doğrulanamadı.");

            if (!await CanJoinBranchChannelAsync(currentUser.Id, branchGuid))
                throw new HubException("Bu şube kanalına katılma yetkiniz yok. Check-in yapın veya yönetici olun.");

            await Groups.AddToGroupAsync(Context.ConnectionId, $"branch:{branchId}");
        }

        public async Task LeaveBranchChannel(string branchId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"branch:{branchId}");
        }

        public async Task JoinRoomGroup(string roomId)
        {
            if (!Guid.TryParse(roomId, out var roomGuid))
                throw new HubException("Geçersiz roomId.");

            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
                throw new HubException("Kullanıcı doğrulanamadı.");

            var room = await _chatRoomRepository.GetByIdWithUsersAsync(roomGuid, Context.ConnectionAborted);
            if (room == null)
                throw new HubException("Oda bulunamadı.");

            if (!await CanCurrentUserJoinRoomAsync(currentUser.Id, room))
                throw new HubException("Bu odaya katılma yetkiniz yok.");

            await Groups.AddToGroupAsync(Context.ConnectionId, $"chatroom:{roomId}");
        }

        public async Task LeaveRoomGroup(string roomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"chatroom:{roomId}");
        }

        private async Task NotifySharedPeersAsync(Guid userId, bool isOnline, DateTime? lastSeenAt)
        {
            var peerUserIds = await _chatRoomRepository.GetSharedRoomPeerUserIdsAsync(userId, Context.ConnectionAborted);
            if (peerUserIds.Count == 0)
                return;

            var identityMap = await _userQueryRepository.GetIdentityIdsByUserIdsAsync(peerUserIds, Context.ConnectionAborted);
            var identityIds = identityMap.Values.Select(id => id.ToString()).ToList();

            await _notificationService.SendNotificationToUsersAsync(
                identityIds,
                "UserPresenceChanged",
                new { UserId = userId, IsOnline = isOnline, LastSeenAt = lastSeenAt });
        }

        private string? GetIdentityIdString() =>
            Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? Context.User?.FindFirst("sub")?.Value;

        private async Task<UserDto?> GetCurrentUserAsync()
        {
            var identityIdClaim = GetIdentityIdString();
            if (string.IsNullOrWhiteSpace(identityIdClaim) || !Guid.TryParse(identityIdClaim, out var identityId))
                return null;

            return await _userQueryRepository.GetByIdentityIdAsync(identityId, Context.ConnectionAborted);
        }

        private async Task<bool> CanJoinBranchChannelAsync(Guid userId, Guid branchId)
        {
            var location = await _userLocationQueryRepository.GetActiveLocationByUserIdAsync(userId, Context.ConnectionAborted);
            if (location != null && location.BranchId == branchId)
                return true;

            return await _branchQueryRepository.CanUserManageBranchAsync(userId, branchId, Context.ConnectionAborted);
        }

        private async Task<bool> CanCurrentUserJoinRoomAsync(Guid userId, ChatRoom room)
        {
            if (room.IsMemberOnlyRoom)
                return room.ChatRoomUserMaps.Any(m => m.UserId == userId);

            var location = await _userLocationQueryRepository.GetActiveLocationByUserIdAsync(userId, Context.ConnectionAborted);
            if (location != null && location.BranchId == room.BranchId)
                return true;

            return await _branchQueryRepository.CanUserManageBranchAsync(userId, room.BranchId, Context.ConnectionAborted);
        }
    }
}
