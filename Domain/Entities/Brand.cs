using Domain.Events.BrandEvents;
using Domain.Exceptions;
using Domain.SeedWork;

namespace Domain.Entities
{
    public class Brand : Entity, IAggregateRoot
    {
        public string Name { get; private set; }
        public string? FileId { get; private set; }
        public Guid OwnerUserId { get; private set; }

        // Navigations
        public User? OwnerUser { get; private set; }
        public ICollection<Branch> Branches { get; private set; } = new List<Branch>();

        private Brand() { }

        public static Brand Create(string name, Guid ownerUserId, string? fileId = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new BrandDomainException("Marka adı (Name) boş olamaz.");

            var brand = new Brand
            {
                Id = Guid.NewGuid(),
                Name = name,
                OwnerUserId = ownerUserId,
                FileId = fileId
            };

            brand.AddDomainEvent(new BrandCreatedDomainEvent(brand.Id, brand.Name));

            return brand;
        }

        public void SetOwner(Guid newOwnerUserId)
        {
            if (newOwnerUserId == Guid.Empty)
                throw new BrandDomainException("Yeni sahip (OwnerUserId) boş olamaz.");

            if (OwnerUserId == newOwnerUserId)
                return;

            var oldOwnerId = OwnerUserId;
            OwnerUserId = newOwnerUserId;

            AddDomainEvent(new BrandOwnerUpdatedDomainEvent(Id, newOwnerUserId, oldOwnerId));
        }

        public void UpdateDetails(string newName, string? newFileId)
        {
            if (string.IsNullOrWhiteSpace(newName))
                throw new BrandDomainException("Marka adı (Name) boş olamaz.");

            Name = newName;
            FileId = newFileId;

            // Güncelleme için de bir event fırlatılabilir (şimdilik eklemedim)
        }
    }
}