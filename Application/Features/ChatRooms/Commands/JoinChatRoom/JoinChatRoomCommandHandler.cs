using Application.Abstractions.Services;
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
        private readonly IChatRoomUserMapRepository _mapRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService; // SignalR için

        public JoinChatRoomCommandHandler(
            IChatRoomRepository chatRoomRepository,
            IChatRoomUserMapRepository mapRepository,
            IUnitOfWork unitOfWork,
            INotificationService notificationService)
        {
            _chatRoomRepository = chatRoomRepository;
            _mapRepository = mapRepository;
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
        }

        public async Task Handle(JoinChatRoomCommand request, CancellationToken cancellationToken)
        {
            // 1. Oda var mı?
            var room = await _chatRoomRepository.GetByIdAsync(request.RoomId, cancellationToken);
            if (room == null)
                throw new Exception("Oda bulunamadı.");

            // 2. Kullanıcı o şubede mi? (Ön koşul)
            if (room.BranchId != request.UserCurrentBranchId)
                throw new Exception("Bu odaya katılmak için önce şubeye check-in yapmalısınız.");

            // 3. Oda public mi? (Private ise davet gerekir - o mantık eklenebilir)
            if (room.RoomType != RoomType.Public)
                throw new Exception("Bu oda gizlidir, sadece davetle girilebilir.");

            // 4. Kullanıcı zaten üye mi?
            var existingMap = await _mapRepository.FindByRoomAndUserAsync(request.RoomId, request.UserId, cancellationToken);
            if (existingMap != null)
                return; // Zaten üye, bir şey yapma

            // 5. Üye yap
            var newMap = ChatRoomUserMap.Create(request.RoomId, request.UserId);
            _mapRepository.Add(newMap);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 6. SignalR'a bildir (Kullanıcıyı gruba ekle)
            // Not: Bu, Hub'ın OnConnected metodunda client'tan gelen
            // 'joinGroup' isteğiyle yapılmalı. Handler'dan yapmak zordur
            // çünkü 'ConnectionId'yi bilmeyiz.

            // Şimdilik, o gruptakilere "X katıldı" bildirimi yollayalım.
            var groupName = $"chatroom:{request.RoomId}";
            await _notificationService.SendNotificationToGroupAsync(
                groupName,
                "UserJoined", // Client'ın dinleyeceği metot
                new { UserId = request.UserId, RoomId = request.RoomId }
            );
        }
    }
}
