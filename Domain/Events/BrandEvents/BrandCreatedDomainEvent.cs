using MediatR;

namespace Domain.Events.BrandEvents
{
    public class BrandCreatedDomainEvent : INotification
    {
        public Guid BrandId { get; }
        public string BrandName { get; }

        public BrandCreatedDomainEvent(Guid brandId, string brandName)
        {
            BrandId = brandId;
            BrandName = brandName;
        }
    }
}
