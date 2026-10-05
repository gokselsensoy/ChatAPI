using Application.Abstractions.QueryRepositories;
using Application.Abstractions.Services;
using Application.Features.Users.DTOs;
using Application.Features.ChatRooms.Commands.ToggleReaction;
using MediatR;
using Domain.Entities;
using Domain.Repositories;
using Domain.SeedWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using Application.Features.ChatRooms.Queries.CheckUserRoomAccess;
using Application.Features.Users.Commands.UpdateUserLastSeen;

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
        private readonly ISender _sender;

        public ChatHub(
            IPresenceService presenceService,
            INotificationService notificationService,
            IUnitOfWork unitOfWork)
        {
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
                _presenceService.RegisterConnection(currentUser.Id, Context.ConnectionId);
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
                _presenceService.UnregisterConnection(currentUser.Id, Context.ConnectionId);
                var becameOffline = _presenceService.SetOffline(currentUser.Id);
                if (becameOffline)
                {
                    var lastSeen = await _sender.Send(new UpdateUserLastSeenCommand { UserId = currentUser.Id }, Context.ConnectionAborted);
                    await NotifySharedPeersAsync(currentUser.Id, isOnline: false, lastSeenAt: lastSeen ?? DateTime.UtcNow);
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

            // Şube yetki kontrolünü MediatR'daki Query üzerinden arka planda yapıyoruz
            var hasAccess = await _sender.Send(new Application.Features.Branches.Queries.CheckUserBranchAccess.CheckUserBranchAccessQuery
            {
                UserId = currentUser.Id,
                BranchId = branchGuid
            }, Context.ConnectionAborted);

            if (!hasAccess)
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
            try
            {
                var hasAccess = await _sender.Send(new CheckUserRoomAccessQuery
                {
                    RoomId = roomGuid,
                    UserId = currentUser.Id
                }, Context.ConnectionAborted);
                if (!hasAccess)
                    throw new HubException("Bu odaya katılma yetkiniz yok.");
            }
            catch (Exception ex)
            {
                throw new HubException(ex.Message);
            }
            await Groups.AddToGroupAsync(Context.ConnectionId, $"chatroom:{roomId}");
        }

        public async Task LeaveRoomGroup(string roomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"chatroom:{roomId}");
        }

        public async Task ToggleReaction(string messageId, string emoji)
        {
            if (!Guid.TryParse(messageId, out var messageGuid)) throw new HubException("Geçersiz mesaj formatı!");

            if (string.IsNullOrWhiteSpace(emoji)) throw new HubException("Eoji boş olamaz.");

            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null) throw new HubException("Kullanıcı doğrulanamadı.");

            var command = new ToggleReactionCommand
            {
                MessageId = messageGuid,
                Emoji = emoji,
                UserId = currentUser.Id
            };

            try
            {
                //İş kuralını MediatR'a devrediyoruz.
                await _sender.Send(command);
            }
            catch (DomainException ex)
            {
                throw new HubException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new HubException("Reaction işlenirken beklenmeyen bir hata oluştu.");
            }
        }

        private async Task NotifySharedPeersAsync(Guid userId, bool isOnline, DateTime? lastSeenAt)
        {
            var identityIds = await _sender.Send(new Application.Features.Users.Queries.GetSharedPeersIdentityIds.GetSharedPeersIdentityIdsQuery
            {
                UserId = userId
            }, Context.ConnectionAborted);

            if (identityIds == null || identityIds.Count == 0)
                return;

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

            // MediatR üzerinden okuma işlemini yap
            return await _sender.Send(new Application.Features.Users.Queries.GetUserByIdentityId.GetUserByIdentityIdQuery
            {
                IdentityId = identityId
            }, Context.ConnectionAborted);
        }
    }
}
