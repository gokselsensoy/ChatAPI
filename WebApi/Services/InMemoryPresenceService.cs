using Application.Abstractions.Services;
using System.Collections.Concurrent;

namespace WebApi.Services
{
    public class InMemoryPresenceService : IPresenceService
    {
        private readonly ConcurrentDictionary<Guid, int> _connections = new();
        private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<string, byte>> _connectionIds = new();

        public bool SetOnline(Guid userId)
        {
            var count = _connections.AddOrUpdate(userId, 1, (_, current) => current + 1);
            return count == 1;
        }

        public bool SetOffline(Guid userId)
        {
            while (true)
            {
                if (!_connections.TryGetValue(userId, out var current))
                    return false;

                if (current <= 1)
                {
                    if (_connections.TryRemove(new KeyValuePair<Guid, int>(userId, current)))
                        return true;

                    continue;
                }

                if (_connections.TryUpdate(userId, current - 1, current))
                    return false;
            }
        }

        public bool IsOnline(Guid userId) =>
            _connections.TryGetValue(userId, out var count) && count > 0;

        public IReadOnlyDictionary<Guid, bool> GetOnlineStatus(IEnumerable<Guid> userIds)
        {
            return userIds
                .Distinct()
                .ToDictionary(id => id, IsOnline);
        }

        public void RegisterConnection(Guid userId, string connectionId)
        {
            if (string.IsNullOrWhiteSpace(connectionId))
                return;

            var set = _connectionIds.GetOrAdd(userId, _ => new ConcurrentDictionary<string, byte>());
            set[connectionId] = 0;
        }

        public void UnregisterConnection(Guid userId, string connectionId)
        {
            if (!_connectionIds.TryGetValue(userId, out var set))
                return;

            set.TryRemove(connectionId, out _);
            if (set.IsEmpty)
                _connectionIds.TryRemove(userId, out _);
        }

        public IReadOnlyList<string> GetConnectionIds(Guid userId)
        {
            if (!_connectionIds.TryGetValue(userId, out var set) || set.IsEmpty)
                return Array.Empty<string>();

            return set.Keys.ToList();
        }
    }
}
