using Domain.SeedWork;
using System;

namespace Domain.Entities
{
    public class BranchEventParticipant : Entity
    {
        public Guid BranchEventId { get; private set; }
        public Guid UserId { get; private set; }
        public bool IsAttending { get; private set; }
        public bool IsReminderSet { get; private set; }
        public string? HangfireJobId { get; private set; }

        // Navigations
        public BranchEvent? BranchEvent { get; private set; }
        public User? User { get; private set; }

        private BranchEventParticipant() { }

        public static BranchEventParticipant Create(Guid branchEventId, Guid userId)
        {
            return new BranchEventParticipant
            {
                Id = Guid.NewGuid(),
                BranchEventId = branchEventId,
                UserId = userId,
                IsAttending = true, // İlk kayıt atıldığında kullanıcı "Katıl" demiştir.
                IsReminderSet = false,
                CreatedDate = DateTime.UtcNow
            };
        }

        public void ToggleParticipation()
        {
            IsAttending = !IsAttending;

            if (!IsAttending)
            {
                IsReminderSet = false;
                HangfireJobId = null;
            }
        }

        public void SetReminder(string jobId)
        {
            IsReminderSet = true;
            HangfireJobId = jobId;
        }

        public void CancelReminder()
        {
            IsReminderSet = false;
            HangfireJobId = null;
        }
    }
}