using MediatR;
using System;

namespace Domain.Events.BranchEvents
{
    public class BranchEventUpdatedDomainEvent : INotification
    {
        public Guid EventId { get; }
        public Guid BranchId { get; }

        public BranchEventUpdatedDomainEvent(Guid eventId, Guid branchId)
        {
            EventId = eventId;
            BranchId = branchId;
        }
    }
}
