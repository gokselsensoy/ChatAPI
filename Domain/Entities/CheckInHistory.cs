using Domain.SeedWork;

namespace Domain.Entities
{
    public class CheckInHistory : Entity, IAggregateRoot
    {
        public Guid UserId { get; private set; }
        public Guid BranchId { get; private set; }
        public DateTime CheckInTime { get; private set; }
        public DateTime? CheckOutTime { get; private set; }

        private CheckInHistory() { }

        public static CheckInHistory Create(Guid userId, Guid branchId)
        {
            return new CheckInHistory
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                BranchId = branchId,
                CheckInTime = DateTime.UtcNow
            };
        }

        public void MarkAsCheckedOut()
        {
            CheckOutTime = DateTime.UtcNow;
        }
    }
}
