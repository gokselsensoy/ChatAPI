using Domain.SeedWork;

namespace Domain.Entities
{
    public class UserDeviceToken : Entity, IAggregateRoot
    {
        public Guid UserId { get; private set; }
        public string Token { get; private set; } = string.Empty;
        public string Platform { get; private set; } = string.Empty; // ios | android

        public User? User { get; private set; }

        private UserDeviceToken() { }

        public static UserDeviceToken Create(Guid userId, string token, string platform)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("UserId geçersiz.");
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Device token boş olamaz.");
            if (string.IsNullOrWhiteSpace(platform))
                throw new ArgumentException("Platform boş olamaz.");

            var normalizedPlatform = platform.Trim().ToLowerInvariant();
            if (normalizedPlatform is not ("ios" or "android"))
                throw new ArgumentException("Platform yalnızca 'ios' veya 'android' olabilir.");

            return new UserDeviceToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Token = token.Trim(),
                Platform = normalizedPlatform,
                CreatedDate = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false
            };
        }

        public void Refresh(Guid userId, string platform)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("UserId geçersiz.");

            var normalizedPlatform = platform.Trim().ToLowerInvariant();
            if (normalizedPlatform is not ("ios" or "android"))
                throw new ArgumentException("Platform yalnızca 'ios' veya 'android' olabilir.");

            UserId = userId;
            Platform = normalizedPlatform;
            UpdatedDate = DateTime.UtcNow;
            IsActive = true;
            IsDeleted = false;
        }

        public void ReassignToUser(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("UserId geçersiz.");

            UserId = userId;
            UpdatedDate = DateTime.UtcNow;
        }

        public void Touch()
        {
            UpdatedDate = DateTime.UtcNow;
            IsActive = true;
            IsDeleted = false;
        }
    }
}
