namespace Application.Abstractions.Services
{
    public interface INotificationService
    {
        Task SendNotificationToUserAsync(string userId, string methodName, object payload);
        Task SendNotificationToAllAsync(string methodName, object payload);
        Task SendNotificationToGroupAsync(string groupName, string methodName, object payload);

        /// <summary>
        /// Oda üyelerine aynı mesajı iletir; gönderen için <paramref name="payloadForSender"/>,
        /// diğerleri için <paramref name="payloadForOthers"/> (ör. IsMine farkı).
        /// <paramref name="senderIdentityId"/> JWT NameIdentifier / sub ile aynı olmalıdır.
        /// </summary>
        Task SendChatRoomMessageToMembersAsync(
            string methodName,
            object payloadForOthers,
            object payloadForSender,
            string senderIdentityId,
            IReadOnlyList<string> otherMemberIdentityIds);
    }
}
