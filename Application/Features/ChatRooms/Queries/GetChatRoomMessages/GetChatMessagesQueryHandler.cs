using Application.Abstractions.QueryRepositories;
using Application.Exceptions;
using Application.Features.ChatRooms.DTOs;
using Application.Shared.Pagination;
using Domain.Entities;
using Domain.Repositories;
using Domain.SeedWork;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Application.Features.ChatRooms.Queries.GetChatRoomMessages
{
    public class GetChatMessagesQueryHandler : IRequestHandler<GetChatMessagesQuery, PaginatedResponse<ChatRoomMessageDto>>
    {
        private readonly IChatRoomRepository _chatRoomRepository;
        private readonly IChatRoomQueryRepository _queryRepository;
        private readonly IUserQueryRepository _userQueryRepository;
        private readonly IBranchQueryRepository _branchQueryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Guid _currentIdentityId;

        public GetChatMessagesQueryHandler(
            IChatRoomRepository chatRoomRepository,
            IChatRoomQueryRepository queryRepository,
            IUserQueryRepository userQueryRepository,
            IBranchQueryRepository branchQueryRepository,
            IUnitOfWork unitOfWork,
            IHttpContextAccessor httpContextAccessor)
        {
            _chatRoomRepository = chatRoomRepository;
            _queryRepository = queryRepository;
            _userQueryRepository = userQueryRepository;
            _branchQueryRepository = branchQueryRepository;
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;

            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;

            if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
                _currentIdentityId = userId;
            else
                _currentIdentityId = Guid.Empty;
        }

        public async Task<PaginatedResponse<ChatRoomMessageDto>> Handle(GetChatMessagesQuery request, CancellationToken cancellationToken)
        {
            if (_currentIdentityId == Guid.Empty)
                throw new UnauthorizedAccessException("Mesajları görmek için giriş yapmalısınız.");

            var currentUserDto = await _userQueryRepository.GetByIdentityIdAsync(_currentIdentityId, cancellationToken);
            if (currentUserDto == null)
                throw new UnauthorizedAccessException("Kullanıcı profili bulunamadı.");

            var currentLocalUserId = currentUserDto.Id;

            var room = await _chatRoomRepository.GetByIdWithUsersAsync(request.RoomId, cancellationToken);
            if (room == null)
                throw new NotFoundException(nameof(ChatRoom), request.RoomId);

            if (room.IsMemberOnlyRoom)
            {
                if (!room.ChatRoomUserMaps.Any(m => m.UserId == currentLocalUserId))
                    throw new UnauthorizedAccessException("Bu özel odayı görme yetkiniz yok.");
            }

            if (room.RequiresCheckInForMessaging)
            {
                var canManage = await _branchQueryRepository.CanUserManageBranchAsync(
                    currentLocalUserId, room.BranchId, cancellationToken);
                if (!canManage && currentUserDto.BranchId != room.BranchId)
                    throw new Exception("Bu sohbeti okumak için şubede check-in olmalısınız.");
            }

            await _chatRoomRepository.MarkAsReadAsync(request.RoomId, currentLocalUserId, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return await _queryRepository.GetMessagesForRoomAsync(
                request.RoomId,
                room.BranchId,
                room.RoomType,
                request,
                currentLocalUserId,
                cancellationToken);
        }
    }
}
