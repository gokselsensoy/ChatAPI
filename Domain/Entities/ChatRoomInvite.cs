using Domain.Enums;
using Domain.SeedWork;

namespace Domain.Entities
{
    public class ChatRoomInvite : Entity, IAggregateRoot
    {
        public Guid ChatRoomId { get; private set; }
        public Guid InviterUserId { get; private set; }
        public Guid InviteeUserId { get; private set; }
        public InviteStatus Status { get; private set; }
        // Navigations
        public ChatRoom ChatRoom { get; private set; }
        public User InviterUser { get; private set; }
        public User InviteeUser { get; private set; }

        private ChatRoomInvite() { }
    }
}