using Application.Abstractions.QueryRepositories;
using Application.Abstractions.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Domain.SeedWork;
using MediatR;

namespace Application.Features.ChatRooms.Commands.CreateGroupRoom
{
    public class CreateGroupRoomCommandHandler : IRequestHandler<CreateGroupRoomCommand, Guid>
    {
        private readonly IChatRoomRepository _chatRoomRepository;
        private readonly IUserQueryRepository _userQueryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;

        public CreateGroupRoomCommandHandler(
            IChatRoomRepository chatRoomRepository,
            IUserQueryRepository userQueryRepository,
            IUnitOfWork unitOfWork,
            INotificationService notificationService)
        {
            _chatRoomRepository = chatRoomRepository;
            _userQueryRepository = userQueryRepository;
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
        }

        public async Task<Guid> Handle(CreateGroupRoomCommand request, CancellationToken cancellationToken)
        {
            // 1. Yeni Private ChatRoom oluştur
            var newGroupRoom = ChatRoom.Create(
                request.Name,
                request.BranchId,
                RoomType.Group,
                request.CreatorUserId
            );
            _chatRoomRepository.Add(newGroupRoom);

            // 2. Davet edilen üyeleri ekle (Kurucu da dahil edilmeli)
            var allMemberIds = request.UserIds.Union(new[] { request.CreatorUserId }).Distinct();

            // (İdeal olarak, tüm üyelerin o şubede olup olmadığı kontrol edilmeli)
            // (Şimdilik bu kontrolü atlıyoruz)

            foreach (var userId in allMemberIds)
            {
                newGroupRoom.AddUser(userId, RoomType.Private, request.BranchId);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 3. SignalR ile tüm üyelere "Yeni gruba eklendiniz" bildirimi yolla
            foreach (var userId in allMemberIds)
            {
                await _notificationService.SendNotificationToUserAsync(
                    userId.ToString(),
                    "AddedToGroup",
                    new { RoomId = newGroupRoom.Id, RoomName = newGroupRoom.Name }
                );
            }

            return newGroupRoom.Id;
        }
    }
}
