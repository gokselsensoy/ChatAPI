using Application.Abstractions.QueryRepositories;
using Application.Abstractions.Services;
using Application.Exceptions;
using Domain.Entities;
using Domain.Repositories;
using Domain.SeedWork;
using MediatR;

namespace Application.Features.ChatRoomInvites.Commands.DeclineChatRoomInvite
{
    public class DeclineChatRoomInviteCommandHandler : IRequestHandler<DeclineChatRoomInviteCommand>
    {
        private readonly IChatRoomInviteRepository _inviteRepository;
        private readonly IUserQueryRepository _userQueryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;

        public DeclineChatRoomInviteCommandHandler(
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

        public async Task Handle(DeclineChatRoomInviteCommand request, CancellationToken cancellationToken)
        {
            var invite = await _inviteRepository.GetByIdAsync(request.InviteId, cancellationToken);
            if (invite == null)
                throw new NotFoundException(nameof(ChatRoomInvite), request.InviteId);

            if (invite.InviteeUserId != request.InviteeUserId)
                throw new Exception("Bu daveti reddetme yetkiniz yok.");

            invite.Decline();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var inviter = await _userQueryRepository.GetByIdAsync(invite.InviterUserId, cancellationToken);
            if (inviter != null)
            {
                await _notificationService.SendNotificationToUserAsync(
                    inviter.IdentityId.ToString(),
                    "InviteDeclined",
                    new { InviteId = invite.Id, InviteeUserId = invite.InviteeUserId });
            }
        }
    }
}
