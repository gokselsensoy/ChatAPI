using Domain.SeedWork;

namespace Domain.Entities
{
    public class BranchAdminMap : Entity
    {
        public Guid BranchId { get; private set; }
        public Guid UserId { get; private set; }

        public Branch? Branch { get; private set; }
        public User? User { get; private set; }

        private BranchAdminMap() { }

        public static BranchAdminMap Create(Guid branchId, Guid userId)
        {
            if (branchId == Guid.Empty || userId == Guid.Empty)
                throw new ArgumentException("BranchId ve UserId geçerli olmalıdır.");

            return new BranchAdminMap
            {
                Id = Guid.NewGuid(),
                BranchId = branchId,
                UserId = userId
            };
        }
    }
}
