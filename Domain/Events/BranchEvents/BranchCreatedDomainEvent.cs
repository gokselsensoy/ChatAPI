using MediatR;
using System;

namespace Domain.Events.BranchEvents
{
    public class BranchEventCreatedDomainEvent : INotification
    {
        public Guid EventId { get; }
        public Guid BranchId { get; }
        public string Title { get; }

        public BranchEventCreatedDomainEvent(Guid eventId, Guid branchId, string title)
        {
            EventId = eventId;
            BranchId = branchId;
            Title = title;
        }
    }
}