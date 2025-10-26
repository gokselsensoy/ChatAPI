using Microsoft.AspNetCore.SignalR;

namespace WebApi.Hubs
{
    // [Authorize]
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            // Context.User.Identity.Name veya Context.User.FindFirst(ClaimTypes.NameIdentifier).Value
            // gibi bir yöntemle kullanıcı ID'sini almalısın (Authentication kurulduktan sonra).
            // var userId = Context.User.Identity.Name;

            // if (!string.IsNullOrEmpty(userId))
            // {
            //     await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            // }

            // Şimdilik adminler için "admin-dashboard" grubu diyelim
            if (Context.GetHttpContext().Request.Query["group"] == "admin")
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "AdminDashboard");
            }

            await base.OnConnectedAsync();
        }

        // İstemciler (client'lar) 'ReceiveNotification' metodunu dinleyecek.
        // Bu metodu biz doğrudan çağırmayacağız, IHubContext üzerinden tetikleyeceğiz.
    }
}