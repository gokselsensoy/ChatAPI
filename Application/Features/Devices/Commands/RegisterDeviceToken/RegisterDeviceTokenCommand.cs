using MediatR;

namespace Application.Features.Devices.Commands.RegisterDeviceToken
{
    public class RegisterDeviceTokenCommand : IRequest<bool>
    {
        public Guid UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
    }
}
