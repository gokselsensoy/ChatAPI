using Application.Abstractions.Messaging;

namespace Application.Features.Users.Commands.CheckOutControl
{
    public class CheckOutControlCommand : ICommand<bool>
    {
        public Guid UserId { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }
}
