using Domain.SeedWork;
using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class BranchEvent : Entity, IAggregateRoot
    {
        public Guid BranchId { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public bool IsHighlighted { get; private set; }
        public string? ImageUrl { get; private set; }

        // Navigations
        public Branch? Branch { get; private set; }
        public ICollection<BranchEventParticipant> Participants { get; private set; }

        private BranchEvent()
        {
            Participants = new List<BranchEventParticipant>();
        }

        // Factory Metot
        public static BranchEvent Create(Guid branchId, string title, string description, DateTime startDate, DateTime endDate, bool isHighlighted = false, string? imageUrl = null)
        {
            if (startDate >= endDate)
                throw new ArgumentException("Bitiş tarihi, başlangıç tarihinden önce veya aynı olamaz.");

            return new BranchEvent
            {
                Id = Guid.NewGuid(),
                BranchId = branchId,
                Title = title,
                Description = description,
                StartDate = startDate,
                EndDate = endDate,
                IsHighlighted = isHighlighted,
                ImageUrl = imageUrl,
                CreatedDate = DateTime.UtcNow,
                Participants = new List<BranchEventParticipant>()
            };
        }
    }
}