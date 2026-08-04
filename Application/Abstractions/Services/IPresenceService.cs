namespace Application.Abstractions.Services
{
    public interface IPresenceService
    {
        /// <summary>Bağlantı sayacını artırır. true = offline→online geçişi.</summary>
        bool SetOnline(Guid userId);

        /// <summary>Bağlantı sayacını azaltır. true = online→offline geçişi.</summary>
        bool SetOffline(Guid userId);

        bool IsOnline(Guid userId);

        IReadOnlyDictionary<Guid, bool> GetOnlineStatus(IEnumerable<Guid> userIds);
    }
}
