using Application.Abstractions.QueryRepositories;
using Application.Abstractions.Services;
using Domain.Enums;
using Domain.Repositories;
using Domain.SeedWork;
using MediatR;

namespace Application.Features.ChatRooms.Commands.LeaveChatRoom
{
    public class LeaveChatRoomCommandHandler : IRequestHandler<LeaveChatRoomCommand>
    {
        private readonly IChatRoomRepository _chatRoomRepository; // DEĞİŞTİ
        private readonly IUserQueryRepository _userQueryRepository; // EKLENDİ
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;

        public LeaveChatRoomCommandHandler(
            IChatRoomRepository chatRoomRepository, // DEĞİŞTİ
            IUnitOfWork unitOfWork,
            IUserQueryRepository userQueryRepository, // EKLENDİ
            INotificationService notificationService)
        {
            _chatRoomRepository = chatRoomRepository;
            _unitOfWork = unitOfWork;
            _userQueryRepository = userQueryRepository; // EKLENDİ
            _notificationService = notificationService;
        }

        public async Task Handle(LeaveChatRoomCommand request, CancellationToken cancellationToken)
        {
            var room = await _chatRoomRepository.GetByIdWithUsersAsync(request.RoomId, cancellationToken);
            if (room == null || room.IsDeleted) return;

            var user = await _userQueryRepository.GetByIdAsync(request.UserId, cancellationToken);
            string userName = user?.UserName ?? "Bir kullanıcı";

            if (room.RoomType == RoomType.Private)
            {
                if (!room.ChatRoomUserMaps.Any(m => m.UserId == request.UserId))
                    return;

                var memberUserIds = room.ChatRoomUserMaps.Select(m => m.UserId).ToList();
                var identityByUserId = await _userQueryRepository.GetIdentityIdsByUserIdsAsync(memberUserIds, cancellationToken);
                var identityIds = identityByUserId.Values.Select(id => id.ToString()).ToList();

                room.SetDeletedPrivateAndGroup();
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var endedPayload = new
                {
                    RoomId = room.Id,
                    EndedByUserId = request.UserId,
                    EndedByUserName = userName
                };

                await _notificationService.SendNotificationToGroupAsync(
                    $"chatroom:{room.Id}",
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
                            RoomId = room.Id,
                            RoomType = room.RoomType.ToString(),
                            BranchId = room.BranchId,
                            Ended = true
                        });
                }

                return;
            }

            // 2. Silme İşlemi (Group / Public)
            room.RemoveUser(request.UserId);

            if (room.IsMemberOnlyRoom && !room.ChatRoomUserMaps.Any())
            {
                room.SetDeleted();
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 3. Bildirim Gönder
            var groupName = $"chatroom:{request.RoomId}";
            await _notificationService.SendNotificationToGroupAsync(
                groupName,
                "UserLeft",
                new
                {
                    UserId = request.UserId,
                    UserName = userName,
                    RoomId = request.RoomId,
                    Message = $"{userName} odadan ayrıldı."
                }
            );
        }
    }
}
