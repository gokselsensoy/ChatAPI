using Application.Abstractions.Messaging;

namespace Application.Features.Users.Commands.CheckOut
{
    public class CheckOutCommand : ICommand<bool>
    {
        public Guid UserId { get; set; } // Controller'dan set edilecek
    }
}
