using MediatR;

namespace Application.Features.Devices.Commands.UnregisterDeviceToken
{
    public class UnregisterDeviceTokenCommand : IRequest<bool>
    {
        public Guid UserId { get; set; }
        public string Token { get; set; } = string.Empty;
    }
}
