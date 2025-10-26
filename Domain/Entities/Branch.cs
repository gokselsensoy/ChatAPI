using Domain.Enums;
using Domain.SeedWork;
using Domain.ValueObjects;

namespace Domain.Entities
{
    public class Branch : Entity, IAggregateRoot
    {
        public string Name { get; private set; }
        public Address Address { get; private set; }
        public string? FileId { get; private set; }
        public BranchType BranchType { get; private set; }
        public Guid BrandId { get; private set; }

        // Navigations
        public Brand? Brand { get; private set; }
        public ICollection<ChatRoom> ChatRooms { get; private set; } = new List<ChatRoom>();
        public ICollection<User> Users { get; private set; } = new List<User>();
        public ICollection<Blacklist> Blacklists { get; private set; } = new List<Blacklist>();
        public ICollection<Menu> Menus { get; private set; } = new List<Menu>();
        public ICollection<Announcement> Announcements { get; private set; } = new List<Announcement>();

        private Branch() { }
    }
}