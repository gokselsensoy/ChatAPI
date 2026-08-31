using Application.Abstractions.QueryRepositories;
using Application.Abstractions.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Domain.SeedWork;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.ChatRoomInvites.Commands.CreateChatRoomInvite
{
    public class CreateChatRoomInviteCommandHandler : IRequestHandler<CreateChatRoomInviteCommand, Guid>
    {
        private const string DefaultGroupName = "Grup Sohbeti";

        private readonly IChatRoomInviteRepository _inviteRepository;
        private readonly IChatRoomRepository _chatRoomRepository;
        private readonly IUserQueryRepository _userQueryRepository;
        private readonly IBranchQueryRepository _branchQueryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly IUserDeviceTokenRepository _deviceTokenRepository;

        public CreateChatRoomInviteCommandHandler(
            IChatRoomInviteRepository inviteRepository,
            IChatRoomRepository chatRoomRepository,
            IUserQueryRepository userQueryRepository,
            IBranchQueryRepository branchQueryRepository,
            IUnitOfWork unitOfWork,
            INotificationService notificationService,
            IPushNotificationService pushNotificationService,
            IUserDeviceTokenRepository deviceTokenRepository)
        {
            _inviteRepository = inviteRepository;
            _chatRoomRepository = chatRoomRepository;
            _userQueryRepository = userQueryRepository;
            _branchQueryRepository = branchQueryRepository;
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _pushNotificationService = pushNotificationService;
            _deviceTokenRepository = deviceTokenRepository;
        }

        public async Task<Guid> Handle(CreateChatRoomInviteCommand request, CancellationToken cancellationToken)
        {
            if (request.TargetRoomType is not (RoomType.Private or RoomType.Group))
                throw new Exception("Davet tipi yalnızca Private veya Group olabilir.");

            var inviteeProfile = await _userQueryRepository.GetByIdAsync(request.InviteeUserId, cancellationToken);
            if (inviteeProfile == null)
                throw new Exception("Davet edilecek kullanıcı bulunamadı.");

            var branchMap = await _userQueryRepository.GetUserBranchMapAsync(
                new[] { request.InviterUserId, request.InviteeUserId },
                cancellationToken);

            var inviterBranchId = branchMap.GetValueOrDefault(request.InviterUserId);
            var inviteeBranchId = branchMap.GetValueOrDefault(request.InviteeUserId);

            if (!inviterBranchId.HasValue
                || !inviteeBranchId.HasValue
                || inviterBranchId != inviteeBranchId
                || inviterBranchId != request.UserCurrentBranchId)
                throw new Exception("Davet göndermek için her iki kullanıcı da aynı şubede olmalıdır.");

            // Test: aynı oda/kişi için birden fazla pending davete izin veriliyor.
            // Sonra: pending varken tekrar atılamaz; reddedilince tekrar atılabilir;
            // private kabul + aktif 1:1 varken atılamaz; silinmiş grup eski kaydı bloklamaz.
            // if (await _inviteRepository.HasPendingInviteAsync(request.InviterUserId, request.InviteeUserId, cancellationToken))
            //     throw new Exception("Bu kullanıcıyla zaten bekleyen bir davetiniz var.");

            var sourceRoom = await _chatRoomRepository.GetByIdAsync(request.PublicChatRoomId, cancellationToken);
            var branch = await _branchQueryRepository.GetByIdAsync(request.UserCurrentBranchId, cancellationToken);
            var inviterProfile = await _userQueryRepository.GetByIdAsync(request.InviterUserId, cancellationToken);

            var invite = ChatRoomInvite.Create(
                request.PublicChatRoomId,
                request.InviterUserId,
                request.InviteeUserId,
                request.TargetRoomType);

            _inviteRepository.Add(invite);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var isGroup = request.TargetRoomType == RoomType.Group;
            var groupName = isGroup
                ? (sourceRoom?.RoomType == RoomType.Group ? sourceRoom.Name : DefaultGroupName)
                : null;

            await _notificationService.SendNotificationToUserAsync(
                inviteeProfile.IdentityId.ToString(),
                "ReceiveInvite",
                new
                {
                    InviteId = invite.Id,
                    InviterUserId = request.InviterUserId,
                    InviterName = inviterProfile?.UserName ?? "Kullanıcı",
                    InviterFirstName = inviterProfile?.FirstName,
                    InviterLastName = inviterProfile?.LastName,
                    InviterFileId = inviterProfile?.FileId,
                    TargetRoomType = request.TargetRoomType.ToString(),
                    PublicChatRoomId = request.PublicChatRoomId,
                    SourceChatRoomName = sourceRoom?.Name,
                    BranchId = request.UserCurrentBranchId,
                    BranchName = branch?.Name,
                    GroupName = groupName
                });

            var tokens = await _deviceTokenRepository.GetActiveTokensByUserIdsAsync(new[] { request.InviteeUserId }, cancellationToken);
            if (tokens.Count > 0)
            {
                string inviterDisplayName = !string.IsNullOrWhiteSpace(inviterProfile?.FirstName) || !string.IsNullOrWhiteSpace(inviterProfile?.LastName)
                    ? $"{inviterProfile.FirstName} {inviterProfile.LastName}".Trim()
                    : (inviterProfile?.UserName ?? "Kullanıcı");

                await _pushNotificationService.SendToTokensAsync(
                    tokens,
                    new PushMessage
                    {
                        Title = "Yeni Davet",
                        Body = $"{inviterDisplayName} size bir sohbet daveti gönderdi.",
                        Data = new Dictionary<string, string>
                        {
                            ["type"] = "invite",
                            ["inviteId"] = invite.Id.ToString(),
                            ["targetRoomType"] = request.TargetRoomType.ToString()
                        }
                    },
                    cancellationToken);
            }

            return invite.Id;
        }
    }
}
