namespace Application.Abstractions.Services
{
    public interface INotificationService
    {
        Task SendNotificationToUserAsync(string userId, string methodName, object payload);
        Task SendNotificationToAllAsync(string methodName, object payload);
        Task SendNotificationToGroupAsync(string groupName, string methodName, object payload);

        /// <summary>
        /// Birden fazla kişisel kanala (identity id grupları) aynı hafif payload'u yollar.
        /// </summary>
        Task SendNotificationToUsersAsync(IEnumerable<string> userIds, string methodName, object payload);
    }
}
