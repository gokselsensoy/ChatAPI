using Domain.SeedWork;

namespace Domain.Entities
{
    public class Blacklist : Entity, IAggregateRoot
    {
        public Guid UserId { get; private set; }
        public Guid BranchId { get; private set; }
        public DateTime? FinishTime { get; private set; }
        public string Reason { get; private set; }

        public User? User { get; private set; }
        public Branch? Branch { get; private set; }

        private Blacklist() { }

        public static Blacklist Create(Guid userId, Guid branchId, string reason, DateTime? finishTime)
        {
            return new Blacklist
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                BranchId = branchId,
                Reason = reason,
                FinishTime = finishTime
            };
        }

        public void UpdateFinishTime(DateTime? newFinishTime)
        {
            FinishTime = newFinishTime;
        }

        // Banı anında kaldırma (Geçmişte kalmasını sağlayarak inaktif yapıyoruz)
        // Veya alternatif olarak doğrudan veritabanından silebilirsin, ama log/geçmiş 
        // tutmak istiyorsan FinishTime'ı şu ana çekmek en mantıklısıdır.
        public void LiftBan()
        {
            FinishTime = DateTime.UtcNow;
        }

        // Ban süresi dolmamışsa veya süresizse TRUE döner. Süre dolduysa otomatik FALSE döner.
        public bool IsActive()
        {
            return !FinishTime.HasValue || FinishTime.Value > DateTime.UtcNow;
        }
    }
}