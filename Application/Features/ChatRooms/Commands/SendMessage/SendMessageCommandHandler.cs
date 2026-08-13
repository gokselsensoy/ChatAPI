using Application.Abstractions.QueryRepositories;
using Application.Abstractions.Services;
using Application.Exceptions;
using Application.Features.ChatRooms.DTOs;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Domain.SeedWork;
using MediatR;

namespace Application.Features.ChatRooms.Commands.SendMessage
{
    public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, ChatRoomMessageDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IChatRoomRepository _chatRoomRepository;
        private readonly INotificationService _notificationService;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly IUserQueryRepository _userQueryRepository;
        private readonly IBlacklistQueryRepository _blacklistQueryRepository;
        private readonly IBranchQueryRepository _branchQueryRepository;
        private readonly IUserDeviceTokenRepository _deviceTokenRepository;
        private readonly IPresenceService _presenceService;

        public SendMessageCommandHandler(
            IUnitOfWork unitOfWork,
            IChatRoomRepository chatRoomRepository,
            INotificationService notificationService,
            IPushNotificationService pushNotificationService,
            IUserQueryRepository userQueryRepository,
            IBlacklistQueryRepository blacklistQueryRepository,
            IBranchQueryRepository branchQueryRepository,
            IUserDeviceTokenRepository deviceTokenRepository,
            IPresenceService presenceService)
        {
            _unitOfWork = unitOfWork;
            _chatRoomRepository = chatRoomRepository;
            _notificationService = notificationService;
            _pushNotificationService = pushNotificationService;
            _userQueryRepository = userQueryRepository;
            _blacklistQueryRepository = blacklistQueryRepository;
            _branchQueryRepository = branchQueryRepository;
            _deviceTokenRepository = deviceTokenRepository;
            _presenceService = presenceService;
        }

        public async Task<ChatRoomMessageDto> Handle(SendMessageCommand request, CancellationToken cancellationToken)
        {
            var room = await _chatRoomRepository.GetByIdWithUsersAsync(request.RoomId, cancellationToken);
            if (room == null)
                throw new NotFoundException(nameof(ChatRoom), request.RoomId);

            bool isBanned = await _blacklistQueryRepository.IsUserBannedAsync(request.SenderUserId, room.BranchId, cancellationToken);
            if (isBanned)
                throw new UnauthorizedAccessException("Bu şubede mesaj göndermeniz engellenmiştir.");

            if (room.IsMemberOnlyRoom && !room.ChatRoomUserMaps.Any(m => m.UserId == request.SenderUserId))
                throw new UnauthorizedAccessException("Bu odaya mesaj gönderme yetkiniz yok.");

            if (room.RequiresCheckInForMessaging)
                await EnsureUserAtBranchOrAdminAsync(request.SenderUserId, room.BranchId, cancellationToken);

            var message = room.AddMessage(request.SenderUserId, request.Message);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var privileged = await _branchQueryRepository.GetBranchPrivilegedUserIdsAsync(room.BranchId, cancellationToken);
            var senderRole = privileged.Contains(message.SenderUserId) ? "Admin" : "Müşteri";

            var messageDto = new ChatRoomMessageDto
            {
                Id = message.Id,
                ChatRoomId = message.ChatRoomId,
                SenderUserId = message.SenderUserId,
                SenderUserName = request.SenderUserName,
                Message = message.Message,
                CreatedDate = message.CreatedDate,
                SenderRole = senderRole,
                IsMine = false
            };

            var previewText = TruncatePreview(message.Message);
            var preview = new ChatRoomPreviewDto
            {
                RoomId = room.Id,
                RoomType = room.RoomType.ToString(),
                BranchId = room.BranchId,
                LastMessagePreview = previewText,
                LastMessageAt = message.CreatedDate,
                SenderUserId = message.SenderUserId,
                HasNew = true,
                UnreadCount = 1
            };

            var senderConnectionIds = _presenceService.GetConnectionIds(request.SenderUserId);
            await _notificationService.SendNotificationToGroupExceptAsync(
                $"chatroom:{room.Id}",
                senderConnectionIds,
                "ReceiveMessage",
                messageDto);

            // Gönderen JoinRoomGroup'ta; grup yayını isMine=false olduğu için ayrı gönderilir
            if (senderConnectionIds.Count > 0)
            {
                var mineDto = new ChatRoomMessageDto
                {
                    Id = messageDto.Id,
                    ChatRoomId = messageDto.ChatRoomId,
                    SenderUserId = messageDto.SenderUserId,
                    SenderUserName = messageDto.SenderUserName,
                    Message = messageDto.Message,
                    CreatedDate = messageDto.CreatedDate,
                    SenderRole = messageDto.SenderRole,
                    IsMine = true
                };
                await _notificationService.SendNotificationToConnectionsAsync(
                    senderConnectionIds,
                    "ReceiveMessage",
                    mineDto);
            }

            if (room.RoomType == RoomType.Public)
            {
                await _notificationService.SendNotificationToGroupAsync(
                    $"branch:{room.BranchId}",
                    "BranchRoomPreviewUpdated",
                    preview);
            }
            else
            {
                var memberUserIds = room.ChatRoomUserMaps.Select(m => m.UserId).ToList();
                var identityByUserId = await _userQueryRepository.GetIdentityIdsByUserIdsAsync(memberUserIds, cancellationToken);
                var identityIds = identityByUserId.Values.Select(id => id.ToString()).ToList();

                if (room.RoomType == RoomType.Private)
                {
                    var peerId = memberUserIds.FirstOrDefault(id => id != request.SenderUserId);
                    preview.PeerUserId = peerId == Guid.Empty ? null : peerId;
                }

                await _notificationService.SendNotificationToUsersAsync(
                    identityIds,
                    "PrivateInboxUpdated",
                    preview);

                // OS push: Private + Group — gönderen hariç
                var recipientUserIds = memberUserIds.Where(id => id != request.SenderUserId).ToList();
                var tokens = await _deviceTokenRepository.GetActiveTokensByUserIdsAsync(recipientUserIds, cancellationToken);
                if (tokens.Count > 0)
                {
                    await _pushNotificationService.SendToTokensAsync(
                        tokens,
                        new PushMessage
                        {
                            Title = request.SenderUserName,
                            Body = previewText ?? "Yeni mesaj",
                            Data = new Dictionary<string, string>
                            {
                                ["type"] = "private_message",
                                ["roomId"] = room.Id.ToString(),
                                ["roomType"] = room.RoomType.ToString(),
                                ["senderUserId"] = request.SenderUserId.ToString()
                            }
                        },
                        cancellationToken);
                }
            }

            messageDto.IsMine = true;
            return messageDto;
        }

        private async Task EnsureUserAtBranchOrAdminAsync(Guid userId, Guid branchId, CancellationToken cancellationToken)
        {
            if (await _branchQueryRepository.CanUserManageBranchAsync(userId, branchId, cancellationToken))
                return;

            var branchMap = await _userQueryRepository.GetUserBranchMapAsync(new[] { userId }, cancellationToken);
            if (!branchMap.TryGetValue(userId, out var userBranchId) || userBranchId != branchId)
                throw new Exception("Bu sohbete devam etmek için şubede check-in olmalısınız.");
        }

        private static string? TruncatePreview(string? message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return null;

            const int maxLen = 120;
            return message.Length <= maxLen ? message : message[..maxLen] + "...";
        }
    }
}
