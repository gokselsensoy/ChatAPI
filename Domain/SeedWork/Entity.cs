using MediatR;

namespace Domain.SeedWork
{
    public abstract class Entity
    {
        public Guid Id { get; protected set; }
        public DateTime CreatedDate { get; protected set; }
        public DateTime? UpdatedDate { get; protected set; }
        public bool IsDeleted { get; protected set; }
        public bool IsActive { get; protected set; }

        public override bool Equals(object? obj)
        {
            if (obj == null || obj is not Entity other)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (GetRealType() != other.GetRealType())
                return false;

            if (Id == Guid.Empty || other.Id == Guid.Empty)
                return false;

            return Id == other.Id;
        }

        public static bool operator ==(Entity a, Entity b)
        {
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
                return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;
            return a.Equals(b);
        }
        public static bool operator !=(Entity a, Entity b) => !(a == b);
        public override int GetHashCode() => (GetRealType().ToString() + Id).GetHashCode();
        private Type GetRealType() => GetType(); // Proxy'lere karşı koruma (EF Core)

        // --- Domain Event (MediatR) Yönetimi ---
        private readonly List<INotification> _domainEvents = new();
        public IReadOnlyCollection<INotification> DomainEvents => _domainEvents.AsReadOnly();

        protected void AddDomainEvent(INotification eventItem)
        {
            _domainEvents.Add(eventItem);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
    }
}
