using Application.Abstractions.QueryRepositories;
using Application.Exceptions;
using Application.Features.ChatRooms.DTOs;
using Application.Shared.Pagination;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
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
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Guid _currentIdentityId;
        public GetChatMessagesQueryHandler(IChatRoomRepository chatRoomRepository, 
            IChatRoomQueryRepository queryRepository, 
            IUserQueryRepository userQueryRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _chatRoomRepository = chatRoomRepository;
            _queryRepository = queryRepository;
            _userQueryRepository = userQueryRepository;
            _httpContextAccessor = httpContextAccessor;

            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;

            if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
            {
                _currentIdentityId = userId;
            }
            else
            {
                _currentIdentityId = Guid.Empty;
            }
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

            // KURAL 2 (GEO-LOCK)
            if (room.RoomType == RoomType.Private)
            {
                // Kullanıcının o anki şubesini al
                var currentUser = await _userQueryRepository.GetByIdAsync(currentLocalUserId, cancellationToken);

                // 1. Kullanıcı o odanın üyesi mi?
                if (!room.ChatRoomUserMaps.Any(m => m.UserId == currentLocalUserId))
                    throw new Exception("Bu özel odayı görme yetkiniz yok.");

                // 2. Kural: Konuşmaya devam etmek için o mekanda olunmalı
                await CheckGeoLockAsync(room, room.BranchId);
            }

            // KURAL 6 (2 SAAT LİMİTİ)
            // Kuralları geçtiyse mesajları getir
            return await _queryRepository.GetMessagesForRoomAsync(
                request.RoomId,
                room.RoomType, // 2 saatlik kural için
                request,
                currentLocalUserId,
                cancellationToken
            );
        }

        private async Task CheckGeoLockAsync(ChatRoom room, Guid requiredBranchId)
        {
            var memberUserIds = room.ChatRoomUserMaps.Select(m => m.UserId);
            var userBranchMap = await _userQueryRepository.GetUserBranchMapAsync(memberUserIds);

            // O şubede (check-in yapmış) olan üyelerin sayısı
            var membersAtBranch = userBranchMap
                .Count(pair => pair.Value.HasValue && pair.Value.Value == requiredBranchId);

            if (room.ChatRoomUserMaps.Count <= 2) // 1-e-1 Chat
            {
                if (membersAtBranch < 2)
                    throw new Exception("Bu özel sohbete devam etmek için her iki kullanıcının da mekanda olması gerekir.");
            }
            else // Grup Chat'i
            {
                if (membersAtBranch < 2)
                    throw new Exception("Bu grup sohbetine devam etmek için en az 2 üyenin mekanda olması gerekir.");
            }
        }
    }
}
