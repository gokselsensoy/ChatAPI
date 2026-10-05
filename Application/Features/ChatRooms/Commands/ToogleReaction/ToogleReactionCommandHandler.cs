using Application.Abstractions.Services;
using Application.Features.ChatRooms.Commands.ToogleReaction;
using Domain.Exceptions;
using Domain.Repositories;
using Domain.SeedWork;
using MediatR;

namespace Application.Features.ChatRooms.Commands.ToggleReaction
{
    public class ToggleReactionCommandHandler : IRequestHandler<ToggleReactionCommand, bool>
    {
        private readonly IChatRoomRepository _chatRoomRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService; // SignalR

        public ToggleReactionCommandHandler(IChatRoomRepository chatRoomRepository, IUnitOfWork unitOfWork, INotificationService notificationService)
        {
            _chatRoomRepository = chatRoomRepository;
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
        }

        public async Task<bool> Handle(ToggleReactionCommand request, CancellationToken cancellationToken)
        {
            // 1. Veritabanından (Repository aracaılığıyla) mesajı al
            var message = await _chatRoomRepository.GetMessageByIdAsync(request.MessageId, cancellationToken) ?? throw new ChatRoomDomainException("Mesaj bulunamadı.");

            // 2. Kullanıcı daha önce bu emojiyi eklemiş mi kontrol et
            bool hasReacted = message.Reactions.Any(r => r.UserId == request.UserId && r.Emoji == request.Emoji);

            bool isAdded = !hasReacted; // Birazdan frontend'e silindi mi yoksa eklendi mi diye haber vermek için tuttuk.

            // 3. İş kuralını işlet (Toggle Mantığı)
            if (hasReacted)
            {
                message.RemoveReaction(request.UserId, request.Emoji);
            }
            else
            {
                message.AddReaction(request.UserId, request.Emoji);
            }

            // 4. Veritanına kaydet (Transaction Commit)
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // SignalR ile real-time push
            string groupName = $"chatroom:{message.ChatRoomId}";

            var payload = new
            {
                MessageId = message.Id,
                UserId = request.UserId,
                Emoji = request.Emoji,
                IsAdded = isAdded
            };

            await _notificationService.SendNotificationToGroupAsync(groupName, "ReactionToggled", payload);

            return true;
        }
    }
}