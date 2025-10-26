using Domain.SeedWork;

namespace Domain.Entities
{
    public class OrderItem : Entity
    {
        public Guid OrderId { get; private set; }
        public Guid ProductId { get; private set; }
        public int Quantity { get; private set; }
        public decimal Price { get; private set; }

        // EF Core için parametresiz constructor
        private OrderItem() { }

        public OrderItem(Guid orderId, Guid productId, int quantity, decimal price)
        {
            // Domain kuralları burada da olabilir
            if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
            if (price <= 0) throw new ArgumentOutOfRangeException(nameof(price));

            OrderId = orderId;
            ProductId = productId;
            Quantity = quantity;
            Price = price;
            Id = Guid.NewGuid();
        }

        public void AddQuantity(int quantity)
        {
            if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
            Quantity += quantity;
        }
    }
}
