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

        public async Task SendChatRoomMessageToMembersAsync(
            string methodName,
            object payloadForOthers,
            object payloadForSender,
            string senderIdentityId,
            IReadOnlyList<string> otherMemberIdentityIds)
        {
            if (otherMemberIdentityIds.Count > 0)
                await _hubContext.Clients.Users(otherMemberIdentityIds).SendAsync(methodName, payloadForOthers);

            await _hubContext.Clients.User(senderIdentityId).SendAsync(methodName, payloadForSender);
        }

        public async Task SendNotificationToUserAsync(string userId, string methodName, object payload)
        {
            await _hubContext.Clients.Group(userId).SendAsync(methodName, payload);
        }
    }
}
