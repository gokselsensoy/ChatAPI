using MediatR;
using System;

namespace Domain.Events.BranchEvents
{
    public class BranchEventDeletedDomainEvent : INotification
    {
        public Guid EventId { get; }
        public Guid BranchId { get; }

        public BranchEventDeletedDomainEvent(Guid eventId, Guid branchId)
        {
            EventId = eventId;
            BranchId = branchId;
        }
    }
}