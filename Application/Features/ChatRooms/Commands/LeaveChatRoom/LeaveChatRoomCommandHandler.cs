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
            if (room == null) return;

            // 1. Kullanıcı ismini SİLMEDEN ÖNCE al (Veya User tablosundan ayrıca çek)
            // ChatRoomUserMaps içinden user'ı bulup ismini alabilirsin eğer Include yaptıysan.
            // Garanti olsun diye UserQueryRepository'den çekelim:
            var user = await _userQueryRepository.GetByIdAsync(request.UserId, cancellationToken);
            string userName = user?.UserName ?? "Bir kullanıcı";

            // 2. Silme İşlemi
            room.RemoveUser(request.UserId);

            if (room.RoomType == RoomType.Private && !room.ChatRoomUserMaps.Any())
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
                    UserName = userName, // <--- İSİM EKLENDİ
                    RoomId = request.RoomId,
                    Message = $"{userName} odadan ayrıldı."
                }
            );
        }
    }
}
