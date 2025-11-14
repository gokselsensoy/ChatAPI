using Application.Abstractions.QueryRepositories;
using Application.Abstractions.Services;
using Application.Exceptions;
using Application.Features.ChatRooms.DTOs;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Domain.SeedWork;
using MediatR;

namespace Application.Features.ChatRooms.Commands.SendMessage
{
    public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, ChatRoomMessageDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IChatRoomRepository _chatRoomRepository; // DEĞİŞTİ
        private readonly INotificationService _notificationService;
        private readonly IUserQueryRepository _userQueryRepository; // YENİ (Geo-Lock için)

        public SendMessageCommandHandler(
            IUnitOfWork unitOfWork,
            IChatRoomRepository chatRoomRepository,
            INotificationService notificationService,
            IUserQueryRepository userQueryRepository) // YENİ
        {
            _unitOfWork = unitOfWork;
            _chatRoomRepository = chatRoomRepository;
            _notificationService = notificationService;
            _userQueryRepository = userQueryRepository; // YENİ
        }

        public async Task<ChatRoomMessageDto> Handle(SendMessageCommand request, CancellationToken cancellationToken)
        {
            // 1. Odayı üye listesiyle birlikte bul
            // (Mesajları include etmeye gerek yok, sadece üye kontrolü lazım)
            var room = await _chatRoomRepository.GetByIdWithUsersAsync(request.RoomId, cancellationToken);
            if (room == null)
                throw new NotFoundException(nameof(ChatRoom), request.RoomId);

            // 2. KURAL 2 (GEO-LOCK)
            if (room.RoomType == RoomType.Private || room.RoomType == RoomType.Group)
            {
                await CheckGeoLockAsync(room, room.BranchId); // (GetMessages'taki yardımcı metot)
            }

            // 2. Domain Metodunu Çağır (İş kuralı kontrolü ve mesaj oluşturma)
            var message = room.AddMessage(request.SenderUserId, request.Message);

            // 3. Kaydet (EF Core yeni eklenen mesajı anlar)
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 4. DTO'yu Hazırla (SignalR için)
            var messageDto = new ChatRoomMessageDto
            {
                Id = message.Id,
                ChatRoomId = message.ChatRoomId,
                SenderUserId = message.SenderUserId,
                SenderUserName = request.SenderUserName,
                Message = message.Message,
                CreatedDate = message.CreatedDate
            };

            // 5. SignalR ile Gruba Yayınla
            var groupName = $"chatroom:{request.RoomId}";
            await _notificationService.SendNotificationToGroupAsync(
                groupName,
                "ReceiveMessage",
                messageDto
            );

            return messageDto;
        }

        // (Bu metot idealde 'IChatAuthorizationService'e taşınır)
        private async Task CheckGeoLockAsync(ChatRoom room, Guid requiredBranchId)
        {
            var memberUserIds = room.ChatRoomUserMaps.Select(m => m.UserId);
            var userBranchMap = await _userQueryRepository.GetUserBranchMapAsync(memberUserIds);

            var membersAtBranch = userBranchMap
                .Count(pair => pair.Value.HasValue && pair.Value.Value == requiredBranchId);

            if (room.ChatRoomUserMaps.Count <= 2 && room.RoomType == RoomType.Private) // 1-e-1 Chat
            {
                if (membersAtBranch < 2)
                    throw new Exception("Bu özel sohbete devam etmek için her iki kullanıcının da mekanda olması gerekir.");
            }
            else // Grup Chat'i
            {
                if (membersAtBranch < 2 && room.RoomType == RoomType.Group)
                    throw new Exception("Bu grup sohbetine devam etmek için en az 2 üyenin mekanda olması gerekir.");
            }
        }
    }
}
