using Application.Abstractions.Services;
using Application.Exceptions;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Domain.SeedWork;
using MediatR;

namespace Application.Features.ChatRooms.Commands.JoinChatRoom
{
    public class JoinChatRoomCommandHandler : IRequestHandler<JoinChatRoomCommand>
    {
        private readonly IChatRoomRepository _chatRoomRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService; // SignalR için

        public JoinChatRoomCommandHandler(
            IChatRoomRepository chatRoomRepository, // DEĞİŞTİ
            IUnitOfWork unitOfWork,
            INotificationService notificationService)
        {
            _chatRoomRepository = chatRoomRepository;
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
        }

        public async Task Handle(JoinChatRoomCommand request, CancellationToken cancellationToken)
        {
            var room = await _chatRoomRepository.GetByIdWithUsersAsync(request.RoomId, cancellationToken);
            if (room == null)
                throw new NotFoundException(nameof(ChatRoom), request.RoomId);

            room.AddUser(request.UserId, room.RoomType, request.UserCurrentBranchId);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var groupName = $"chatroom:{request.RoomId}";
            await _notificationService.SendNotificationToGroupAsync(
                groupName,
                "UserJoined",
                new { UserId = request.UserId, RoomId = request.RoomId }
            );
        }
    }
}
