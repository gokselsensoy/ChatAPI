using Domain.Exceptions;
using Domain.SeedWork;

namespace Domain.Entities
{
    public class UserBlock : Entity, IAggregateRoot
    {
        public Guid BlockerUserId { get; private set; }
        public Guid BlockedUserId { get; private set; }

        public User? BlockerUser { get; private set; }
        public User? BlockedUser { get; private set; }

        private UserBlock() { }

        public static UserBlock Create(Guid blockerUserId, Guid blockedUserId)
        {
            if (blockerUserId == Guid.Empty || blockedUserId == Guid.Empty)
                throw new UserDomainException("Kullanıcı ID'leri boş olamaz.");

            if (blockerUserId == blockedUserId)
                throw new UserDomainException("Kullanıcı kendi kendini engelleyemez.");

            return new UserBlock
            {
                Id = Guid.NewGuid(),
                BlockerUserId = blockerUserId,
                BlockedUserId = blockedUserId,
                CreatedDate = DateTime.UtcNow
            };
        }
    }
}
