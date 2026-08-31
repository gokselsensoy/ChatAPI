using Application.Abstractions.Messaging;
namespace Application.Features.Users.Commands.CheckIn
{
    public class CheckInCommand : ICommand<bool>
    {
        public Guid BranchId { get; set; }
        public Guid UserId { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }
}
