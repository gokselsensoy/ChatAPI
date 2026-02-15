using Application.Abstractions.Services;
using Microsoft.AspNetCore.SignalR;
using WebApi.Hubs;

namespace WebApi.Services
{
    public class SignalRNotificationService : INotificationService
    {
        private readonly IHubContext<ChatHub> _hubContext;

        public SignalRNotificationService(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendNotificationToAllAsync(string methodName, object payload)
        {
            await _hubContext.Clients.All.SendAsync(methodName, payload);
        }

        public async Task SendNotificationToGroupAsync(string groupName, string methodName, object payload)
        {
            await _hubContext.Clients.Group(groupName).SendAsync(methodName, payload);
        }

        public async Task SendNotificationToUserAsync(string userId, string methodName, object payload)
        {
            await _hubContext.Clients.Group(userId).SendAsync(methodName, payload);
        }
    }
}
