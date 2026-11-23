using Domain.Enums;
using Domain.Exceptions;
using Domain.SeedWork;

namespace Domain.Entities
{
    public class UserReport : Entity, IAggregateRoot
    {
        public Guid ReporterUserId { get; private set; }
        public Guid ReportedUserId { get; private set; }
        public Guid? ChatRoomId { get; private set; }
        public Guid? MessageId { get; private set; }
        public ReportReason Reason { get; private set; }
        public string? Description { get; private set; }

        public User? ReporterUser { get; private set; }
        public User? ReportedUser { get; private set; }
        public ChatRoom? ChatRoom { get; private set; }
        public ChatRoomMessage? Message { get; private set; }

        private UserReport() { }

        public static UserReport Create(
            Guid reporterUserId,
            Guid reportedUserId,
            ReportReason reason,
            string? description = null,
            Guid? chatRoomId = null,
            Guid? messageId = null)
        {
            if (reporterUserId == Guid.Empty || reportedUserId == Guid.Empty)
                throw new UserDomainException("Kullanıcı ID'leri boş olamaz.");

            if (reporterUserId == reportedUserId)
                throw new UserDomainException("Kullanıcı kendi kendini şikayet edemez.");

            var trimmedDescription = string.IsNullOrWhiteSpace(description)
                ? null
                : description.Trim();

            if (trimmedDescription != null && trimmedDescription.Length > 500)
                trimmedDescription = trimmedDescription[..500];

            return new UserReport
            {
                Id = Guid.NewGuid(),
                ReporterUserId = reporterUserId,
                ReportedUserId = reportedUserId,
                Reason = reason,
                Description = trimmedDescription,
                ChatRoomId = chatRoomId,
                MessageId = messageId,
                CreatedDate = DateTime.UtcNow
            };
        }
    }
}
