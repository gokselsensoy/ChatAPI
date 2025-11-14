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
        // private readonly IChatRoomUserMapRepository _mapRepository; // SİLİNDİ
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;

        public LeaveChatRoomCommandHandler(
            IChatRoomRepository chatRoomRepository, // DEĞİŞTİ
            IUnitOfWork unitOfWork,
            INotificationService notificationService)
        {
            _chatRoomRepository = chatRoomRepository;
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
        }

        public async Task Handle(LeaveChatRoomCommand request, CancellationToken cancellationToken)
        {
            var room = await _chatRoomRepository.GetByIdWithUsersAsync(request.RoomId, cancellationToken);

            if (room == null)
                return;

            room.RemoveUser(request.UserId);

            if (room.RoomType == RoomType.Private && !room.ChatRoomUserMaps.Any())
            {
                room.SetDeleted();
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var groupName = $"chatroom:{request.RoomId}";
            await _notificationService.SendNotificationToGroupAsync(
                groupName,
                "UserLeft",
                new { UserId = request.UserId, RoomId = request.RoomId }
            );
        }
    }
}
