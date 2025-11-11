using Application.Abstractions.Services;
using Domain.Repositories;
using Domain.SeedWork;
using MediatR;

namespace Application.Features.ChatRooms.Commands.LeaveChatRoom
{
    public class LeaveChatRoomCommandHandler : IRequestHandler<LeaveChatRoomCommand>
    {
        private readonly IChatRoomUserMapRepository _mapRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;

        public LeaveChatRoomCommandHandler(
            IChatRoomUserMapRepository mapRepository,
            IUnitOfWork unitOfWork,
            INotificationService notificationService)
        {
            _mapRepository = mapRepository;
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
        }

        public async Task Handle(LeaveChatRoomCommand request, CancellationToken cancellationToken)
        {
            var existingMap = await _mapRepository.FindByRoomAndUserAsync(request.RoomId, request.UserId, cancellationToken);

            if (existingMap == null)
                return; // Zaten üye değil

            // Fikrinizdeki gibi, sadece map kaydını siliyoruz.
            _mapRepository.Delete(existingMap);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // SignalR'a bildir
            var groupName = $"chatroom:{request.RoomId}";
            await _notificationService.SendNotificationToGroupAsync(
                groupName,
                "UserLeft",
                new { UserId = request.UserId, RoomId = request.RoomId }
            );
        }
    }
}
