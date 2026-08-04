using Application.Abstractions.QueryRepositories;
using Application.Abstractions.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Domain.SeedWork;
using MediatR;

namespace Application.Features.ChatRooms.Commands.CreateGroupRoom
{
    public class CreateGroupRoomCommandHandler : IRequestHandler<CreateGroupRoomCommand, Guid>
    {
        private readonly IChatRoomRepository _chatRoomRepository;
        private readonly IUserQueryRepository _userQueryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;

        public CreateGroupRoomCommandHandler(
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

        public async Task<Guid> Handle(CreateGroupRoomCommand request, CancellationToken cancellationToken)
        {
            var newGroupRoom = ChatRoom.Create(
                request.Name,
                request.BranchId,
                RoomType.Group);

            _chatRoomRepository.Add(newGroupRoom);

            var allMemberIds = request.UserIds.Union(new[] { request.CreatorUserId }).Distinct().ToList();

            foreach (var userId in allMemberIds)
                newGroupRoom.JoinViaInvite(userId, request.BranchId);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var identityMap = await _userQueryRepository.GetIdentityIdsByUserIdsAsync(allMemberIds, cancellationToken);
            foreach (var userId in allMemberIds)
            {
                if (!identityMap.TryGetValue(userId, out var identityId))
                    continue;

                await _notificationService.SendNotificationToUserAsync(
                    identityId.ToString(),
                    "AddedToGroup",
                    new { RoomId = newGroupRoom.Id, RoomName = newGroupRoom.Name, RoomType = RoomType.Group.ToString() });
            }

            return newGroupRoom.Id;
        }
    }
}
