using Domain.SeedWork;

namespace Domain.Entities
{
    public class Blacklist : Entity
    {
        public Guid UserId { get; private set; }
        public Guid BranchId { get; private set; }
        public DateTime? FinishTime { get; private set; }
        public string Reason { get; private set; }

        // Navigations
        public User? User { get; private set; }
        public Branch? Branch { get; private set; }

        private Blacklist() { }
    }
}