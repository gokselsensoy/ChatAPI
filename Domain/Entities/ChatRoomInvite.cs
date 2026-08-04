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

        /// <summary>Accept sonrası: Private (geo'suz 1:1) veya Group (geo'lu).</summary>
        public RoomType TargetRoomType { get; private set; }

        public Guid? PrivateChatRoomId { get; private set; }

        public ChatRoom? ChatRoom { get; private set; }
        public User? InviterUser { get; private set; }
        public User? InviteeUser { get; private set; }

        private ChatRoomInvite() { }

        public static ChatRoomInvite Create(
            Guid chatRoomId,
            Guid inviterUserId,
            Guid inviteeUserId,
            RoomType targetRoomType)
        {
            if (inviterUserId == inviteeUserId)
                throw new ChatRoomDomainException("Kullanıcı kendini davet edemez.");

            if (targetRoomType is not (RoomType.Private or RoomType.Group))
                throw new ChatRoomDomainException("Davet yalnızca Private veya Group için olabilir.");

            return new ChatRoomInvite
            {
                Id = Guid.NewGuid(),
                ChatRoomId = chatRoomId,
                InviterUserId = inviterUserId,
                InviteeUserId = inviteeUserId,
                TargetRoomType = targetRoomType,
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
        }

        public void Decline()
        {
            if (Status != InviteStatus.Pending)
                throw new ChatRoomDomainException("Bu davet zaten yanıtlanmış.");

            Status = InviteStatus.Declined;
            UpdatedDate = DateTime.UtcNow;
        }
    }
}
