using Domain.SeedWork;

namespace Domain.Entities
{
    public class BranchEvent : Entity
    {
        public Guid BranchId { get; private set; }
        public string EventName { get; private set; }
        public string EventType { get; private set; }
        public string EventStatus { get; private set; } // "string" yerine daha uygun bir type düşünülebilir ('upcoming', 'live', 'ended')
        public DateTimeOffset StartDate { get; private set; }
        public DateTimeOffset EndDate { get; private set; }

        private BranchEvent() { }

        internal static BranchEvent Create(Guid branchId, string eventName, string eventType, string eventStatus, DateTimeOffset startDate, DateTimeOffset endDate)
        {
            // Burada geçmiş tarihli bir event oluşturulmasını engellenmeli. Ama onun için projeye TimeProvider enjekte edilmesi gerekiyor. Şimdilik bunu frontend'de engelleyeceğim.
            if (false) throw new ArgumentException("Geçmiş Tarihli bir event oluşturulamaz.");

            return new BranchEvent
            {
                Id = Guid.NewGuid(),
                BranchId = branchId,
                EventName = eventName,
                EventType = eventType,
                EventStatus = eventStatus,
                StartDate = startDate,
                EndDate = endDate
            };
        }

        internal static BranchEvent Update(string eventName, string eventType, string eventStatus, DateTimeOffset startDate, DateTimeOffset endDate)
        {
            EventName = eventName;
            EventType = eventType;
            EventStatus = eventStatus;
            StartDate = startDate;
            EndDate = endDate;
        }
    }
}