using Application.Abstractions.QueryRepositories;
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
        private readonly IUserQueryRepository _userQueryRepository; // Kullanıcı bilgisi için
        private readonly INotificationService _notificationService; // SignalR için

        public JoinChatRoomCommandHandler(
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

        public async Task Handle(JoinChatRoomCommand request, CancellationToken cancellationToken)
        {
            var room = await _chatRoomRepository.GetByIdWithUsersAsync(request.RoomId, cancellationToken);
            if (room == null)
                throw new NotFoundException(nameof(ChatRoom), request.RoomId);

            // 1. İşlemi Yap
            room.AddUser(request.UserId, room.RoomType, request.UserCurrentBranchId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 2. Kullanıcı Adını Bul (Bildirim için gerekli)
            // Eğer User repository'n varsa oradan, yoksa query repository'den çekebilirsin.
            var user = await _userQueryRepository.GetByIdAsync(request.UserId, cancellationToken);
            string userName = user?.UserName ?? "Bir kullanıcı"; // Null check

            // 3. Bildirimi Zenginleştirilmiş Veriyle Gönder
            var groupName = $"chatroom:{request.RoomId}";
            await _notificationService.SendNotificationToGroupAsync(
                groupName,
                "UserJoined",
                new
                {
                    UserId = request.UserId,
                    UserName = userName, // <--- ARTIK İSİM DE GİDİYOR
                    RoomId = request.RoomId,
                    Message = $"{userName} odaya katıldı." // İstersen hazır mesaj da yollayabilirsin
                }
            );
        }
    }
}
