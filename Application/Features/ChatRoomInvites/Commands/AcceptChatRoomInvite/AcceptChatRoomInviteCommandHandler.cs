using Application.Abstractions.QueryRepositories;
using Application.Abstractions.Services;
using Application.Exceptions;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Domain.SeedWork;
using MediatR;

namespace Application.Features.ChatRoomInvites.Commands.AcceptChatRoomInvite
{
    public class AcceptChatRoomInviteCommandHandler : IRequestHandler<AcceptChatRoomInviteCommand, Guid>
    {
        private readonly IChatRoomInviteRepository _inviteRepository;
        private readonly IChatRoomRepository _chatRoomRepository;
        private readonly IUserQueryRepository _userQueryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;

        public AcceptChatRoomInviteCommandHandler(
            IChatRoomInviteRepository inviteRepository,
            IChatRoomRepository chatRoomRepository,
            IUserQueryRepository userQueryRepository,
            IUnitOfWork unitOfWork,
            INotificationService notificationService)
        {
            _inviteRepository = inviteRepository;
            _chatRoomRepository = chatRoomRepository;
            _userQueryRepository = userQueryRepository;
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
        }

        public async Task<Guid> Handle(AcceptChatRoomInviteCommand request, CancellationToken cancellationToken)
        {
            var invite = await _inviteRepository.GetByIdWithRoomAsync(request.InviteId, cancellationToken);
            if (invite == null)
                throw new NotFoundException(nameof(ChatRoomInvite), request.InviteId);

            if (invite.InviteeUserId != request.InviteeUserId)
                throw new Exception("Bu daveti kabul etme yetkiniz yok.");

            var targetType = invite.TargetRoomType is RoomType.Private or RoomType.Group
                ? invite.TargetRoomType
                : RoomType.Private;

            Guid roomId;

            if (targetType == RoomType.Private)
            {
                var existing = await _chatRoomRepository.FindDirectRoomBetweenUsersAsync(
                    invite.InviterUserId,
                    invite.InviteeUserId,
                    RoomType.Private,
                    cancellationToken);

                if (existing != null)
                {
                    invite.Accept(existing.Id);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    roomId = existing.Id;
                }
                else
                {
                    roomId = await CreateRoomAndJoinAsync(invite, targetType, cancellationToken);
                }
            }
            else
            {
                roomId = await CreateRoomAndJoinAsync(invite, targetType, cancellationToken);
            }

            var inviter = await _userQueryRepository.GetByIdAsync(invite.InviterUserId, cancellationToken);
            if (inviter != null)
            {
                await _notificationService.SendNotificationToUserAsync(
                    inviter.IdentityId.ToString(),
                    "InviteAccepted",
                    new
                    {
                        InviteId = invite.Id,
                        NewRoomId = roomId,
                        RoomType = targetType.ToString(),
                        InviteeUserId = invite.InviteeUserId
                    });
            }

            return roomId;
        }

        private async Task<Guid> CreateRoomAndJoinAsync(
            ChatRoomInvite invite,
            RoomType targetType,
            CancellationToken cancellationToken)
        {
            if (invite.ChatRoom == null)
                throw new Exception("Davetin bağlı olduğu public oda yüklenemedi.");

            var roomName = targetType == RoomType.Private ? "Özel Sohbet" : "Grup Sohbeti";
            var newRoom = ChatRoom.Create(
                roomName,
                invite.ChatRoom.BranchId,
                targetType);

            _chatRoomRepository.Add(newRoom);
            invite.Accept(newRoom.Id);

            if (targetType == RoomType.Group)
            {
                var branchMap = await _userQueryRepository.GetUserBranchMapAsync(
                    new[] { invite.InviterUserId, invite.InviteeUserId },
                    cancellationToken);

                var inviterBranchId = branchMap.GetValueOrDefault(invite.InviterUserId);
                var inviteeBranchId = branchMap.GetValueOrDefault(invite.InviteeUserId);

                if (inviterBranchId != newRoom.BranchId || inviteeBranchId != newRoom.BranchId)
                    throw new Exception("Grup odasına katılmak için her iki kullanıcı da şubede check-in olmalıdır.");

                newRoom.JoinViaInvite(invite.InviterUserId, inviterBranchId.Value);
                newRoom.JoinViaInvite(invite.InviteeUserId, inviteeBranchId.Value);
            }
            else
            {
                // Private geo'suz: check-in şart değil.
                newRoom.JoinViaInvite(invite.InviterUserId, newRoom.BranchId);
                newRoom.JoinViaInvite(invite.InviteeUserId, newRoom.BranchId);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return newRoom.Id;
        }
    }
}
