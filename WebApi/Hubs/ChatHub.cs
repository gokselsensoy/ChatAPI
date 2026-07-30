using Application.Abstractions.QueryRepositories;
using Application.Features.Users.DTOs;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace WebApi.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IUserQueryRepository _userQueryRepository;
        private readonly IChatRoomRepository _chatRoomRepository;
        private readonly IUserLocationQueryRepository _userLocationQueryRepository;
        private readonly IBranchQueryRepository _branchQueryRepository;

        public ChatHub(
            IUserQueryRepository userQueryRepository,
            IChatRoomRepository chatRoomRepository,
            IUserLocationQueryRepository userLocationQueryRepository,
            IBranchQueryRepository branchQueryRepository)
        {
            _userQueryRepository = userQueryRepository;
            _chatRoomRepository = chatRoomRepository;
            _userLocationQueryRepository = userLocationQueryRepository;
            _branchQueryRepository = branchQueryRepository;
        }

        public override async Task OnConnectedAsync()
        {
            // Kişisel kanal: PrivateInboxUpdated buraya gelir (identity id = JWT sub/NameIdentifier)
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? Context.User?.FindFirst("sub")?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            }

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Branch chat listesi açıkken çağır. Her odaya join etmeden BranchRoomPreviewUpdated almak için.
        /// </summary>
        public async Task JoinBranchChannel(string branchId)
        {
            if (!Guid.TryParse(branchId, out var branchGuid))
                throw new HubException("Geçersiz branchId.");

            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
                throw new HubException("Kullanıcı doğrulanamadı.");

            var canJoin = await CanJoinBranchChannelAsync(currentUser.Id, branchGuid);
            if (!canJoin)
                throw new HubException("Bu şube kanalına katılma yetkiniz yok. Check-in yapın veya yönetici olun.");

            await Groups.AddToGroupAsync(Context.ConnectionId, $"branch:{branchId}");
        }

        public async Task LeaveBranchChannel(string branchId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"branch:{branchId}");
        }

        /// <summary>
        /// Sadece açık sohbet ekranında çağır. ReceiveMessage buraya akar.
        /// </summary>
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

            var canJoin = await CanCurrentUserJoinRoomAsync(currentUser.Id, room);
            if (!canJoin)
                throw new HubException("Bu odaya katılma yetkiniz yok.");

            await Groups.AddToGroupAsync(Context.ConnectionId, $"chatroom:{roomId}");
        }

        public async Task LeaveRoomGroup(string roomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"chatroom:{roomId}");
        }

        private async Task<UserDto?> GetCurrentUserAsync()
        {
            var identityIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? Context.User?.FindFirst("sub")?.Value;

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
            if (room.RoomType == RoomType.Private || room.RoomType == RoomType.Group)
            {
                return room.ChatRoomUserMaps.Any(m => m.UserId == userId);
            }

            var location = await _userLocationQueryRepository.GetActiveLocationByUserIdAsync(userId, Context.ConnectionAborted);
            if (location != null && location.BranchId == room.BranchId)
                return true;

            return await _branchQueryRepository.CanUserManageBranchAsync(userId, room.BranchId, Context.ConnectionAborted);
        }
    }
}
