using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace WebApi.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        // Kullanıcı uygulama açıldığında otomatik bağlanır.
        public override async Task OnConnectedAsync()
        {
            // Token'dan User ID'yi alıp kendi özel grubuna ekleyelim.
            // Böylece "SendNotificationToUserAsync" çalışır.
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? Context.User?.FindFirst("sub")?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            }

            await base.OnConnectedAsync();
        }

        // --- EKSİK OLAN PARÇA BURASI ---

        // Mobil uygulama Chat ekranını açtığı an bu metodu çağırmalı!
        public async Task JoinRoomGroup(string roomId)
        {
            var groupName = $"chatroom:{roomId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        // Mobil uygulama Chat ekranından çıktığı an (Geri tuşu) bu metodu çağırmalı!
        public async Task LeaveRoomGroup(string roomId)
        {
            var groupName = $"chatroom:{roomId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }
    }
}