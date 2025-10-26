using MediatR;

namespace Domain.Events
{
    public class OrderUpdatedDomainEvent : INotification
    {
        public Guid OrderId { get; }
        public Guid CustomerId { get; }

        public OrderUpdatedDomainEvent(Guid orderId, Guid customerId)
        {
            OrderId = orderId;
            CustomerId = customerId;
        }
    }
}
