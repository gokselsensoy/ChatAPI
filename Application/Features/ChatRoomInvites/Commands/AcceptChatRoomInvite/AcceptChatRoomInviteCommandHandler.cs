using Application.Abstractions.Services;
using Application.Exceptions;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Domain.SeedWork;
using MediatR;

namespace Application.Features.ChatRoomInvites.Commands.AcceptChatRoomInvite
{
    public class AcceptChatRoomInviteCommandHandler : IRequestHandler<AcceptChatRoomInviteCommand, Guid>
    {
        private readonly IChatRoomInviteRepository _inviteRepository;
        private readonly IChatRoomRepository _chatRoomRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;

        public AcceptChatRoomInviteCommandHandler(
            IChatRoomInviteRepository inviteRepository,
            IChatRoomRepository chatRoomRepository,
            IUnitOfWork unitOfWork,
            INotificationService notificationService)
        {
            _inviteRepository = inviteRepository;
            _chatRoomRepository = chatRoomRepository;
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
        }

        public async Task<Guid> Handle(AcceptChatRoomInviteCommand request, CancellationToken cancellationToken)
        {
            // TransactionPipeline tüm bu işlemleri tek bir transaction'da yapacak.

            // 1. Daveti bul
            var invite = await _inviteRepository.GetByIdAsync(request.InviteId, cancellationToken);
            if (invite == null)
                throw new NotFoundException(nameof(ChatRoomInvite), request.InviteId);

            // 2. Güvenlik: Daveti kabul eden, davet edilen kişi mi?
            if (invite.InviteeUserId != request.InviteeUserId)
                throw new Exception("Bu daveti kabul etme yetkiniz yok.");

            // 3. Yeni bir Private ChatRoom oluştur
            var newPrivateRoom = ChatRoom.Create(
                "Özel Sohbet", // (İsimler sonradan düzenlenebilir)
                invite.ChatRoom.BranchId, // Public odanın şubesi
                RoomType.Private
                //invite.InviterUserId // Odayı davet eden oluşturmuş sayılır
            );
            _chatRoomRepository.Add(newPrivateRoom);

            // 4. Daveti "Kabul Edildi" olarak işaretle
            invite.Accept(newPrivateRoom.Id);
            // _inviteRepository.Update(invite) -> (Gerek yok, tracked by UoW)

            // 5. Her iki kullanıcıyı da yeni odaya ekle
            // (AR üzerinden)
            newPrivateRoom.AddUser(invite.InviterUserId, RoomType.Private, newPrivateRoom.BranchId);
            newPrivateRoom.AddUser(invite.InviteeUserId, RoomType.Private, newPrivateRoom.BranchId);

            // 6. Kaydet
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 7. SignalR ile davet edene "Kabul Edildi" bildirimi yolla
            await _notificationService.SendNotificationToUserAsync(
                invite.InviterUserId.ToString(),
                "InviteAccepted",
                new { InviteId = invite.Id, NewRoomId = newPrivateRoom.Id }
            );

            return newPrivateRoom.Id;
        }
    }
}
