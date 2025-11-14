using Application.Abstractions.QueryRepositories;
using Application.Abstractions.Services;
using Domain.Entities;
using Domain.Repositories;
using Domain.SeedWork;
using MediatR;

namespace Application.Features.ChatRoomInvites.Commands.CreateChatRoomInvite
{
    public class CreateChatRoomInviteCommandHandler : IRequestHandler<CreateChatRoomInviteCommand, Guid>
    {
        private readonly IChatRoomInviteRepository _inviteRepository;
        private readonly IUserQueryRepository _userQueryRepository; // Davet edilenin şubesini kontrol için
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService; // SignalR

        public CreateChatRoomInviteCommandHandler(
            IChatRoomInviteRepository inviteRepository,
            IUserQueryRepository userQueryRepository,
            IUnitOfWork unitOfWork,
            INotificationService notificationService)
        {
            _inviteRepository = inviteRepository;
            _userQueryRepository = userQueryRepository;
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
        }

        public async Task<Guid> Handle(CreateChatRoomInviteCommand request, CancellationToken cancellationToken)
        {
            // Kural 1: Davet edilen kullanıcı da aynı şubede mi?
            var inviteeProfile = await _userQueryRepository.GetByIdAsync(request.InviteeUserId, cancellationToken);
            if (inviteeProfile == null || inviteeProfile.BranchId != request.UserCurrentBranchId)
                throw new Exception("Davet göndermek için her iki kullanıcı da aynı şubede olmalıdır.");

            // Kural 2: Zaten bekleyen bir davet var mı?
            if (await _inviteRepository.HasPendingInviteAsync(request.InviterUserId, request.InviteeUserId, cancellationToken))
                throw new Exception("Bu kullanıcıyla zaten bekleyen bir davetiniz var.");

            // Daveti oluştur
            var invite = ChatRoomInvite.Create(
                request.PublicChatRoomId,
                request.InviterUserId,
                request.InviteeUserId
            );

            _inviteRepository.Add(invite);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // SignalR ile davet edilen kullanıcıya "Davet Aldın" bildirimi yolla
            await _notificationService.SendNotificationToUserAsync(
                request.InviteeUserId.ToString(), // (UserId'yi SignalR grubu olarak kullanıyoruz)
                "ReceiveInvite", // Client'ın dinleyeceği metot
                new { InviteId = invite.Id, InviterName = "Kullanıcı Adı" } // (InviterName'i de eklemeliyiz)
            );

            return invite.Id;
        }
    }
}
