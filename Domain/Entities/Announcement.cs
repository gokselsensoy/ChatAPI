using Domain.Enums;
using Domain.SeedWork;

namespace Domain.Entities
{
    public class Announcement : Entity, IAggregateRoot
    {
        public string Title { get; private set; }
        public string Content { get; private set; }
        public AnnouncementType AnnouncementType { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public Guid BranchId { get; private set; }

        // Navigations
        public Branch? Branch { get; private set; }

        private Announcement() { }
    }
}