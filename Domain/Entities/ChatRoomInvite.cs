using Domain.Enums;
using Domain.Exceptions;
using Domain.SeedWork;

namespace Domain.Entities
{
    public class ChatRoomInvite : Entity, IAggregateRoot
    {
        public Guid ChatRoomId { get; private set; }
        public Guid InviterUserId { get; private set; }
        public Guid InviteeUserId { get; private set; }
        public InviteStatus Status { get; private set; }

        public Guid? PrivateChatRoomId { get; private set; }

        // Navigations
        public ChatRoom? ChatRoom { get; private set; }
        public User? InviterUser { get; private set; }
        public User? InviteeUser { get; private set; }

        private ChatRoomInvite() { }

        public static ChatRoomInvite Create(Guid chatRoomId, Guid inviterUserId, Guid inviteeUserId)
        {
            if (inviterUserId == inviteeUserId)
                throw new ChatRoomDomainException("Kullanıcı kendini davet edemez.");

            return new ChatRoomInvite
            {
                Id = Guid.NewGuid(),
                ChatRoomId = chatRoomId,
                InviterUserId = inviterUserId,
                InviteeUserId = inviteeUserId,
                Status = InviteStatus.Pending,
                CreatedDate = DateTime.UtcNow
            };
        }

        public void Accept(Guid privateChatRoomId)
        {
            if (Status != InviteStatus.Pending)
                throw new ChatRoomDomainException("Bu davet zaten yanıtlanmış.");

            Status = InviteStatus.Accepted;
            PrivateChatRoomId = privateChatRoomId;
            UpdatedDate = DateTime.UtcNow;
            // event fırlatılabilir
        }

        public void Decline()
        {
            if (Status != InviteStatus.Pending)
                throw new ChatRoomDomainException("Bu davet zaten yanıtlanmış.");

            Status = InviteStatus.Declined;
            UpdatedDate = DateTime.UtcNow;
            // event fırlatılabilir
        }
    }
}