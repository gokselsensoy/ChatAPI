using Application.Abstractions.QueryRepositories;
using Application.Abstractions.Services;
using Application.Exceptions;
using Application.Features.ChatRooms.DTOs;
using Domain.Entities;
using Domain.Repositories;
using MediatR;

namespace Application.Features.ChatRooms.Queries.GetChatRoomMembers
{
    public class GetChatRoomMembersQueryHandler : IRequestHandler<GetChatRoomMembersQuery, List<ChatRoomMemberDto>>
    {
        private readonly IChatRoomRepository _chatRoomRepository;
        private readonly IChatRoomQueryRepository _chatRoomQueryRepository;
        private readonly IUserQueryRepository _userQueryRepository;
        private readonly IBranchQueryRepository _branchQueryRepository;
        private readonly IPresenceService _presenceService;

        public GetChatRoomMembersQueryHandler(
            IChatRoomRepository chatRoomRepository,
            IChatRoomQueryRepository chatRoomQueryRepository,
            IUserQueryRepository userQueryRepository,
            IBranchQueryRepository branchQueryRepository,
            IPresenceService presenceService)
        {
            _chatRoomRepository = chatRoomRepository;
            _chatRoomQueryRepository = chatRoomQueryRepository;
            _userQueryRepository = userQueryRepository;
            _branchQueryRepository = branchQueryRepository;
            _presenceService = presenceService;
        }

        public async Task<List<ChatRoomMemberDto>> Handle(GetChatRoomMembersQuery request, CancellationToken cancellationToken)
        {
            var room = await _chatRoomRepository.GetByIdWithUsersAsync(request.RoomId, cancellationToken);
            if (room == null)
                throw new NotFoundException(nameof(ChatRoom), request.RoomId);

            var requester = await _userQueryRepository.GetByIdAsync(request.RequestingUserId, cancellationToken);
            if (requester == null)
                throw new UnauthorizedAccessException("Kullanıcı bulunamadı.");

            if (room.IsMemberOnlyRoom)
            {
                if (!room.ChatRoomUserMaps.Any(m => m.UserId == request.RequestingUserId))
                    throw new UnauthorizedAccessException("Bu odanın üyelerini görme yetkiniz yok.");
            }
            else
            {
                // Public: check-in veya branch admin
                var canManage = await _branchQueryRepository.CanUserManageBranchAsync(
                    request.RequestingUserId, room.BranchId, cancellationToken);
                if (!canManage && requester.BranchId != room.BranchId)
                    throw new UnauthorizedAccessException("Üyeleri görmek için şubede check-in olmalısınız.");
            }

            var members = await _chatRoomQueryRepository.GetMembersForRoomAsync(request.RoomId, cancellationToken);
            var onlineMap = _presenceService.GetOnlineStatus(members.Select(m => m.UserId));

            foreach (var member in members)
            {
                member.IsMe = member.UserId == request.RequestingUserId;
                member.IsOnline = onlineMap.TryGetValue(member.UserId, out var online) && online;
            }

            return members
                .OrderByDescending(m => m.IsOnline)
                .ThenBy(m => m.UserName)
                .ToList();
        }
    }
}
