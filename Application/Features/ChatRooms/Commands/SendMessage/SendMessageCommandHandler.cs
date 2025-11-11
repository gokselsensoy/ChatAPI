using Application.Abstractions.Services;
using Application.Features.ChatRooms.DTOs;
using Domain.Entities;
using Domain.Repositories;
using Domain.SeedWork;
using MediatR;

namespace Application.Features.ChatRooms.Commands.SendMessage
{
    public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, ChatRoomMessageDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IChatRoomUserMapRepository _mapRepository;
        // private readonly ApplicationDbContext _context; // KALDIRILDI (Mimari İhlal)
        private readonly IChatRoomMessageRepository _messageRepository; // YENİ
        private readonly INotificationService _notificationService;
        // IMapper'ı (kullanmıyorsak) kaldırabiliriz

        public SendMessageCommandHandler(
            IUnitOfWork unitOfWork,
            IChatRoomUserMapRepository mapRepository,
            IChatRoomMessageRepository messageRepository, // DEĞİŞTİ
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _mapRepository = mapRepository;
            _messageRepository = messageRepository; // DEĞİŞTİ
            _notificationService = notificationService;
        }

        public async Task<ChatRoomMessageDto> Handle(SendMessageCommand request, CancellationToken cancellationToken)
        {
            var map = await _mapRepository.FindByRoomAndUserAsync(request.RoomId, request.SenderUserId, cancellationToken);
            if (map == null)
                throw new Exception("Mesaj göndermek için önce odaya katılmalısınız.");

            var message = ChatRoomMessage.Create(
                request.RoomId,
                request.SenderUserId,
                request.Message
            );

            // 3. Mesajı Repository Aracılığıyla Ekle
            _messageRepository.Add(message); // DEĞİŞTİ
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var messageDto = new ChatRoomMessageDto
            {
                Id = message.Id,
                ChatRoomId = message.ChatRoomId,
                SenderUserId = message.SenderUserId,
                SenderUserName = request.SenderUserName,
                Message = message.Message,
                CreatedDate = message.CreatedDate
            };

            var groupName = $"chatroom:{request.RoomId}";
            await _notificationService.SendNotificationToGroupAsync(
                groupName,
                "ReceiveMessage",
                messageDto
            );

            return messageDto;
        }
    }
}
