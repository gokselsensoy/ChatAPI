namespace Application.Features.Orders.Queries.Shared.DTOs
{
    public class OrderDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }

        // Address'ten gelen flat-mapping
        public string Street { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }

        public List<OrderItemDto> OrderItems { get; set; } = new();
    }
}
