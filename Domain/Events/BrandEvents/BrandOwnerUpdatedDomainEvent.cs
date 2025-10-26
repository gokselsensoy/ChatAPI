using MediatR;

namespace Domain.Events.BrandEvents
{
    public class BrandOwnerUpdatedDomainEvent : INotification
    {
        public Guid BrandId { get; }
        public Guid NewOwnerUserId { get; }
        public Guid OldOwnerUserId { get; }

        public BrandOwnerUpdatedDomainEvent(Guid brandId, Guid newOwnerUserId, Guid oldOwnerUserId)
        {
            BrandId = brandId;
            NewOwnerUserId = newOwnerUserId;
            OldOwnerUserId = oldOwnerUserId;
        }
    }
}
