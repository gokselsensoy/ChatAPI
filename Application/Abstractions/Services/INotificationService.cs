namespace Application.Abstractions.Services
{
    public interface INotificationService
    {
        Task SendNotificationToUserAsync(string userId, string methodName, object payload);
        Task SendNotificationToAllAsync(string methodName, object payload);
        Task SendNotificationToGroupAsync(string groupName, string methodName, object payload);
    }
}
