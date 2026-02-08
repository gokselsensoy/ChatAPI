using Domain.SeedWork;

namespace Domain.Entities
{
    public class UserLocation : Entity, IAggregateRoot
    {
        public Guid UserId { get; private set; }
        public Guid BranchId { get; private set; }
        public DateTime CheckInTime { get; private set; }

        // Navigation
        public virtual User User { get; private set; }
        public virtual Branch Branch { get; private set; }

        // Private constructor (EF Core için)
        private UserLocation() { }

        // Factory Method (Yeni oluşturma)
        public static UserLocation Create(Guid userId, Guid branchId)
        {
            return new UserLocation
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                BranchId = branchId,
                CheckInTime = DateTime.UtcNow
            };
        }

        // Update Method (Mevcut konumu güncelleme)
        public void UpdateLocation(Guid newBranchId)
        {
            // Eğer zaten aynı şubedeyse sadece zamanı güncelle (isteğe bağlı)
            if (BranchId != newBranchId)
            {
                BranchId = newBranchId;
                CheckInTime = DateTime.UtcNow;
            }
        }
    }
}
