using Application.Abstractions.QueryRepositories;
using Application.Abstractions.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Domain.SeedWork;
using MediatR;

namespace Application.Features.Blacklists.Commands.BanUser
{
    public class BanUserCommandHandler : IRequestHandler<BanUserCommand, bool>
    {
        private readonly IRepository<Blacklist> _blacklistRepo;
        private readonly IRepository<UserLocation> _locationRepo;
        private readonly IChatRoomRepository _chatRoomRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;
        private readonly IUserQueryRepository _userQueryRepo;
        private readonly IBranchQueryRepository _branchQueryRepository;

        public BanUserCommandHandler(
            IRepository<Blacklist> blacklistRepo,
            IRepository<UserLocation> locationRepo,
            IChatRoomRepository chatRoomRepo,
            IUnitOfWork unitOfWork,
            INotificationService notificationService,
            IUserQueryRepository userQueryRepo,
            IBranchQueryRepository branchQueryRepository)
        {
            _blacklistRepo = blacklistRepo;
            _locationRepo = locationRepo;
            _chatRoomRepo = chatRoomRepo;
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _userQueryRepo = userQueryRepo;
            _branchQueryRepository = branchQueryRepository;
        }

        public async Task<bool> Handle(BanUserCommand request, CancellationToken cancellationToken)
        {
            if (!await _branchQueryRepository.CanUserManageBranchAsync(request.ActingUserId, request.BranchId, cancellationToken))
                throw new UnauthorizedAccessException("Bu şube için kullanıcıyı uzaklaştırma yetkiniz yok.");

            var blacklist = Blacklist.Create(request.UserId, request.BranchId, request.Reason, request.FinishTime);
            _blacklistRepo.Add(blacklist);

            var location = await _locationRepo.GetAsync(l => l.UserId == request.UserId && l.BranchId == request.BranchId, cancellationToken);
            if (location != null)
            {
                _locationRepo.Delete(location);
            }

            var activeRooms = await _chatRoomRepo.GetRoomsByUserAndBranchAsync(request.UserId, request.BranchId, cancellationToken);

            var user = await _userQueryRepo.GetByIdAsync(request.UserId, cancellationToken);
            string userName = user?.UserName ?? "Bir kullanıcı";

            foreach (var room in activeRooms)
            {
                room.RemoveUser(request.UserId);

                if (room.IsMemberOnlyRoom && !room.ChatRoomUserMaps.Any())
                    room.SetDeleted();

                await _notificationService.SendNotificationToGroupAsync(
                    $"chatroom:{room.Id}",
                    "UserLeft",
                    new { UserId = request.UserId, UserName = userName, RoomId = room.Id, Message = $"{userName} odadan ayrıldı." }
                );
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (user != null)
            {
                await _notificationService.SendNotificationToUserAsync(
                    user.IdentityId.ToString(),
                    "BannedFromBranch",
                    new { BranchId = request.BranchId, Reason = request.Reason });
            }

            return true;
        }
    }
}
