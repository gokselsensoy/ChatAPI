using Application.Abstractions.QueryRepositories;
using Application.Abstractions.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Domain.SeedWork;
using MediatR;

namespace Application.Features.ChatRoomInvites.Commands.CreateChatRoomInvite
{
    public class CreateChatRoomInviteCommandHandler : IRequestHandler<CreateChatRoomInviteCommand, Guid>
    {
        private readonly IChatRoomInviteRepository _inviteRepository;
        private readonly IUserQueryRepository _userQueryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;

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
            if (request.TargetRoomType is not (RoomType.Private or RoomType.Group))
                throw new Exception("Davet tipi yalnızca Private veya Group olabilir.");

            var inviteeProfile = await _userQueryRepository.GetByIdAsync(request.InviteeUserId, cancellationToken);
            if (inviteeProfile == null || inviteeProfile.BranchId != request.UserCurrentBranchId)
                throw new Exception("Davet göndermek için her iki kullanıcı da aynı şubede olmalıdır.");

            if (await _inviteRepository.HasPendingInviteAsync(request.InviterUserId, request.InviteeUserId, cancellationToken))
                throw new Exception("Bu kullanıcıyla zaten bekleyen bir davetiniz var.");

            var inviterProfile = await _userQueryRepository.GetByIdAsync(request.InviterUserId, cancellationToken);

            var invite = ChatRoomInvite.Create(
                request.PublicChatRoomId,
                request.InviterUserId,
                request.InviteeUserId,
                request.TargetRoomType);

            _inviteRepository.Add(invite);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _notificationService.SendNotificationToUserAsync(
                inviteeProfile.IdentityId.ToString(),
                "ReceiveInvite",
                new
                {
                    InviteId = invite.Id,
                    InviterUserId = request.InviterUserId,
                    InviterName = inviterProfile?.UserName ?? "Kullanıcı",
                    InviterFileId = inviterProfile?.FileId,
                    TargetRoomType = request.TargetRoomType.ToString(),
                    PublicChatRoomId = request.PublicChatRoomId
                });

            return invite.Id;
        }
    }
}
