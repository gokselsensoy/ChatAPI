using Application.Abstractions.Messaging;

namespace Application.Features.Orders.Commands.UpdateAddress
{
    public class UpdateOrderAddressCommand : ICommand
    {
        public Guid OrderId { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
    }
}
