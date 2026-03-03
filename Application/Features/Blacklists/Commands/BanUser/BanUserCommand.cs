using Application.Abstractions.QueryRepositories;
using Application.Abstractions.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Domain.SeedWork;
using MediatR;

namespace Application.Features.Blacklists.Commands.BanUser
{
    public class BanUserCommand : IRequest<bool>
    {
        public Guid UserId { get; set; }
        public Guid BranchId { get; set; }
        public string Reason { get; set; }
        public DateTime? FinishTime { get; set; }
    }

    public class BanUserCommandHandler : IRequestHandler<BanUserCommand, bool>
    {
        private readonly IRepository<Blacklist> _blacklistRepo;
        private readonly IRepository<UserLocation> _locationRepo;
        private readonly IChatRoomRepository _chatRoomRepo; // Odaları çekmek için
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;
        private readonly IUserQueryRepository _userQueryRepo;

        public BanUserCommandHandler(IRepository<Blacklist> blacklistRepo, IRepository<UserLocation> locationRepo) 
        {
        }

        public async Task<bool> Handle(BanUserCommand request, CancellationToken cancellationToken)
        {
            // 1. Blacklist Kaydını Oluştur
            var blacklist = Blacklist.Create(request.UserId, request.BranchId, request.Reason, request.FinishTime);
            _blacklistRepo.Add(blacklist);

            // 2. Kullanıcı O An Mekandaysa Check-Out Yap! (UserLocation'dan sil)
            var location = await _locationRepo.GetAsync(l => l.UserId == request.UserId && l.BranchId == request.BranchId, cancellationToken);
            if (location != null)
            {
                _locationRepo.Delete(location);
            }

            // 3. Kullanıcının O Şubedeki Aktif Odalarından Atılması
            // (Şubeye ait ve kullanıcının içinde olduğu odaları çeken bir metot yazmalısın)
            var activeRooms = await _chatRoomRepo.GetRoomsByUserAndBranchAsync(request.UserId, request.BranchId, cancellationToken);

            var user = await _userQueryRepo.GetByIdAsync(request.UserId, cancellationToken);
            string userName = user?.UserName ?? "Bir kullanıcı";

            foreach (var room in activeRooms)
            {
                room.RemoveUser(request.UserId);

                if (room.RoomType == RoomType.Private && !room.ChatRoomUserMaps.Any())
                    room.SetDeleted();

                // Odadaki diğer kişilere "Kullanıcı ayrıldı" bildirimi
                await _notificationService.SendNotificationToGroupAsync(
                    $"chatroom:{room.Id}",
                    "UserLeft",
                    new { UserId = request.UserId, UserName = userName, RoomId = room.Id, Message = $"{userName} odadan ayrıldı." }
                );
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 4. Banlanan Kişiye Özel "Banlandın" Bildirimi (Uygulama onu anasayfaya atsın diye)
            await _notificationService.SendNotificationToUserAsync(
                request.UserId.ToString(),
                "BannedFromBranch",
                new { BranchId = request.BranchId, Reason = request.Reason }
            );

            return true;
        }
    }
}
