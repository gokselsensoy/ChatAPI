using Microsoft.AspNetCore.Authorization;
using Application.Features.Users.DTOs;
using Microsoft.AspNetCore.SignalR;
using Application.Abstractions.QueryRepositories;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
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

        // Kullanıcı uygulama açıldığında otomatik bağlanır.
        public override async Task OnConnectedAsync()
        {
            // Token'dan User ID'yi alıp kendi özel grubuna ekleyelim.
            // Böylece "SendNotificationToUserAsync" çalışır.
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? Context.User?.FindFirst("sub")?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            }

            await base.OnConnectedAsync();
        }

        // --- EKSİK OLAN PARÇA BURASI ---

        // Mobil uygulama Chat ekranını açtığı an bu metodu çağırmalı!
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

            var groupName = $"chatroom:{roomId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        // Mobil uygulama Chat ekranından çıktığı an (Geri tuşu) bu metodu çağırmalı!
        public async Task LeaveRoomGroup(string roomId)
        {
            var groupName = $"chatroom:{roomId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        private async Task<UserDto?> GetCurrentUserAsync()
        {
            var identityIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? Context.User?.FindFirst("sub")?.Value;

            if (string.IsNullOrWhiteSpace(identityIdClaim) || !Guid.TryParse(identityIdClaim, out var identityId))
                return null;

            return await _userQueryRepository.GetByIdentityIdAsync(identityId, Context.ConnectionAborted);
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