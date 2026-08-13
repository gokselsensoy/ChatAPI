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

        public async Task SendNotificationToGroupExceptAsync(
            string groupName,
            IReadOnlyList<string> excludedConnectionIds,
            string methodName,
            object payload)
        {
            if (excludedConnectionIds == null || excludedConnectionIds.Count == 0)
            {
                await _hubContext.Clients.Group(groupName).SendAsync(methodName, payload);
                return;
            }

            await _hubContext.Clients.GroupExcept(groupName, excludedConnectionIds)
                .SendAsync(methodName, payload);
        }

        public async Task SendNotificationToConnectionsAsync(
            IReadOnlyList<string> connectionIds,
            string methodName,
            object payload)
        {
            if (connectionIds == null || connectionIds.Count == 0)
                return;

            await _hubContext.Clients.Clients(connectionIds).SendAsync(methodName, payload);
        }

        public async Task SendNotificationToUserAsync(string userId, string methodName, object payload)
        {
            // OnConnectedAsync identity id'yi Groups'a ekler
            await _hubContext.Clients.Group(userId).SendAsync(methodName, payload);
        }

        public async Task SendNotificationToUsersAsync(IEnumerable<string> userIds, string methodName, object payload)
        {
            var ids = userIds.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList();
            if (ids.Count == 0)
                return;

            await _hubContext.Clients.Groups(ids).SendAsync(methodName, payload);
        }
    }
}
