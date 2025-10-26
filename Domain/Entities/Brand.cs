using Domain.SeedWork;

namespace Domain.Entities
{
    public class Brand : Entity, IAggregateRoot
    {
        public string Name { get; private set; }
        public string FileId { get; private set; }
        public Guid OwnerUserId { get; private set; }

        // Navigations
        public User OwnerUser { get; private set; }
        public ICollection<Branch> Branches { get; private set; } = new List<Branch>();

        private Brand() { }
    }
}