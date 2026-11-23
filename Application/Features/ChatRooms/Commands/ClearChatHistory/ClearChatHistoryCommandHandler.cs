using Application.Abstractions.QueryRepositories;
using Application.Abstractions.Services;
using Application.Exceptions;
using Domain.Entities;
using Domain.Repositories;
using Domain.SeedWork;
using MediatR;

namespace Application.Features.ChatRooms.Commands.ClearChatHistory
{
    public class ClearChatHistoryCommandHandler : IRequestHandler<ClearChatHistoryCommand>
    {
        private readonly IChatRoomRepository _chatRoomRepository;
        private readonly IUserQueryRepository _userQueryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;

        public ClearChatHistoryCommandHandler(
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

        public async Task Handle(ClearChatHistoryCommand request, CancellationToken cancellationToken)
        {
            var room = await _chatRoomRepository.GetByIdWithUsersAsync(request.RoomId, cancellationToken);
            if (room == null || room.IsDeleted)
                throw new NotFoundException(nameof(ChatRoom), request.RoomId);

            var userMap = await _chatRoomRepository.GetUserMapAsync(request.RoomId, request.UserId, cancellationToken);
            if (userMap == null)
                throw new UnauthorizedAccessException("Bu sohbette üyeliğiniz bulunamadı.");

            userMap.ClearHistory();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var user = await _userQueryRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user != null)
            {
                await _notificationService.SendNotificationToUserAsync(
                    user.IdentityId.ToString(),
                    "PrivateInboxUpdated",
                    new
                    {
                        RoomId = room.Id,
                        RoomType = room.RoomType.ToString(),
                        BranchId = room.BranchId,
                        Cleared = true
                    });
            }
        }
    }
}
