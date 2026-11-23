using Application.Abstractions.QueryRepositories;
using Application.Abstractions.Services;
using Application.Exceptions;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Domain.SeedWork;
using MediatR;

namespace Application.Features.Users.Commands.BlockUser
{
    public class BlockUserCommandHandler : IRequestHandler<BlockUserCommand>
    {
        private readonly IUserBlockRepository _userBlockRepository;
        private readonly IChatRoomRepository _chatRoomRepository;
        private readonly IChatRoomInviteRepository _chatRoomInviteRepository;
        private readonly IUserQueryRepository _userQueryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;

        public BlockUserCommandHandler(
            IUserBlockRepository userBlockRepository,
            IChatRoomRepository chatRoomRepository,
            IChatRoomInviteRepository chatRoomInviteRepository,
            IUserQueryRepository userQueryRepository,
            IUnitOfWork unitOfWork,
            INotificationService notificationService)
        {
            _userBlockRepository = userBlockRepository;
            _chatRoomRepository = chatRoomRepository;
            _chatRoomInviteRepository = chatRoomInviteRepository;
            _userQueryRepository = userQueryRepository;
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
        }

        public async Task Handle(BlockUserCommand request, CancellationToken cancellationToken)
        {
            var blockedUser = await _userQueryRepository.GetByIdAsync(request.BlockedUserId, cancellationToken);
            if (blockedUser == null)
                throw new NotFoundException(nameof(User), request.BlockedUserId);

            var existingBlock = await _userBlockRepository.GetBlockAsync(
                request.BlockerUserId,
                request.BlockedUserId,
                cancellationToken);

            if (existingBlock == null)
            {
                var block = UserBlock.Create(request.BlockerUserId, request.BlockedUserId);
                _userBlockRepository.Add(block);
            }

            var pendingInvites = await _chatRoomInviteRepository.GetPendingInvitesBetweenUsersAsync(
                request.BlockerUserId,
                request.BlockedUserId,
                cancellationToken);

            foreach (var invite in pendingInvites)
            {
                invite.Decline();
            }

            var activePrivateRoom = await _chatRoomRepository.FindDirectRoomBetweenUsersAsync(
                request.BlockerUserId,
                request.BlockedUserId,
                RoomType.Private,
                cancellationToken);

            if (activePrivateRoom != null)
            {
                activePrivateRoom.SetDeletedPrivateAndGroup();
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (activePrivateRoom != null)
            {
                var blockerUser = await _userQueryRepository.GetByIdAsync(request.BlockerUserId, cancellationToken);
                var identityByUserId = await _userQueryRepository.GetIdentityIdsByUserIdsAsync(
                    new[] { request.BlockerUserId, request.BlockedUserId },
                    cancellationToken);

                var identityIds = identityByUserId.Values.Select(id => id.ToString()).ToList();

                var endedPayload = new
                {
                    RoomId = activePrivateRoom.Id,
                    EndedByUserId = request.BlockerUserId,
                    EndedByUserName = blockerUser?.UserName ?? "Bir kullanıcı"
                };

                await _notificationService.SendNotificationToGroupAsync(
                    $"chatroom:{activePrivateRoom.Id}",
                    "PrivateChatEnded",
                    endedPayload);

                if (identityIds.Count > 0)
                {
                    await _notificationService.SendNotificationToUsersAsync(
                        identityIds,
                        "PrivateChatEnded",
                        endedPayload);

                    await _notificationService.SendNotificationToUsersAsync(
                        identityIds,
                        "PrivateInboxUpdated",
                        new
                        {
                            RoomId = activePrivateRoom.Id,
                            RoomType = activePrivateRoom.RoomType.ToString(),
                            BranchId = activePrivateRoom.BranchId,
                            Ended = true
                        });
                }
            }
        }
    }
}
