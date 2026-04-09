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
        private readonly IBlacklistQueryRepository _blacklistQueryRepository;
        private readonly IUserLocationQueryRepository _userLocationQueryRepo;
        private readonly IBranchQueryRepository _branchQueryRepository;

        public JoinChatRoomCommandHandler(
            IChatRoomRepository chatRoomRepository, // DEĞİŞTİ
            IUnitOfWork unitOfWork,
            IUserQueryRepository userQueryRepository, // EKLENDİ
            INotificationService notificationService,
            IBlacklistQueryRepository blacklistQueryRepository,
            IUserLocationQueryRepository userLocationQueryRepo,
            IBranchQueryRepository branchQueryRepository)
        {
            _chatRoomRepository = chatRoomRepository;
            _unitOfWork = unitOfWork;
            _userQueryRepository = userQueryRepository; // EKLENDİ
            _notificationService = notificationService;
            _blacklistQueryRepository = blacklistQueryRepository;
            _userLocationQueryRepo = userLocationQueryRepo;
            _branchQueryRepository = branchQueryRepository;
        }

        public async Task Handle(JoinChatRoomCommand request, CancellationToken cancellationToken)
        {
            var realUserId = request.UserId; // Gerçek ID olduğunu varsayıyorum

            var room = await _chatRoomRepository.GetByIdWithUsersAsync(request.RoomId, cancellationToken);
            if (room == null)
                throw new NotFoundException(nameof(ChatRoom), request.RoomId);

            bool isBanned = await _blacklistQueryRepository.IsUserBannedAsync(realUserId, room.BranchId, cancellationToken);
            if (isBanned)
                throw new UnauthorizedAccessException("Bu şubedeki sohbetlere katılmanız engellenmiştir.");

            // YENİ EKLENEN KISIM: Kullanıcının anlık konumunu bul
            var currentLocation = await _userLocationQueryRepo.GetAsync(ul => ul.UserId == realUserId, cancellationToken);
            if (currentLocation == null)
            {
                var canManageBranch = await _branchQueryRepository.CanUserManageBranchAsync(realUserId, room.BranchId, cancellationToken);
                if (!canManageBranch)
                    throw new UnauthorizedAccessException("Bu odaya katılmak için önce bir şubeye check-in yapmalısınız.");
            }

            // 1. İşlemi Yap (Artık yeni Domain metodumuzu ve DB'den gelen BranchId'yi kullanıyoruz)
            room.JoinPublicRoom(realUserId, currentLocation?.BranchId ?? room.BranchId);

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
