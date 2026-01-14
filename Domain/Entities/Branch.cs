using Domain.Enums;
using Domain.Events.BranchEvents;
using Domain.Events.BrandEvents;
using Domain.Exceptions;
using Domain.SeedWork;
using Domain.ValueObjects;

namespace Domain.Entities
{
    public class Branch : Entity, IAggregateRoot
    {
        public string Name { get; private set; }
        public Address Address { get; private set; }
        public string? FileId { get; private set; }
        public BranchType BranchType { get; private set; }
        public Guid BrandId { get; private set; }

        // Navigations
        public Brand? Brand { get; private set; }
        public ICollection<ChatRoom> ChatRooms { get; private set; } = new List<ChatRoom>();
        public ICollection<User> Users { get; private set; } = new List<User>();
        public ICollection<Blacklist> Blacklists { get; private set; } = new List<Blacklist>();
        public ICollection<Menu> Menus { get; private set; } = new List<Menu>();
        public ICollection<Announcement> Announcements { get; private set; } = new List<Announcement>();

        private Branch() { }

        public static Branch Create(
            string name,
            Guid brandId,
            Address address, // Adres VO'su dışarıda oluşturulup buraya verilir
            BranchType branchType,
            string? fileId = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new BranchDomainException("Şube adı (Name) boş olamaz.");

            if (brandId == Guid.Empty)
                throw new BranchDomainException("Şube bir Markaya (BrandId) bağlı olmalıdır.");

            if (address == null)
                throw new BranchDomainException("Adres bilgisi (Address) boş olamaz.");

            // Lat/Long zorunluluğunu VO içinde veya burada kontrol edebilirsiniz
            if (address.Location == null || address.Location.IsEmpty)
                throw new BranchDomainException("Konum bilgisi (Location) girilmelidir.");

            var branch = new Branch
            {
                Id = Guid.NewGuid(),
                Name = name,
                BrandId = brandId,
                Address = address,
                BranchType = branchType,
                FileId = fileId
            };

            branch.AddDomainEvent(new BranchCreatedDomainEvent(branch.Id, branch.BrandId, branch.Name));

            return branch;
        }

        public static Branch Update(
            string name,
            Guid brandId,
            Guid branchId,
            Address address,
            BranchType branchType,
            string? fileId = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new BranchDomainException("Şube adı (Name) boş olamaz.");

            if (brandId == Guid.Empty)
                throw new BranchDomainException("Şube bir Markaya (BrandId) bağlı olmalıdır.");

            if (address == null) 
                throw new BranchDomainException("Adres bilgisi (Address) boş olamaz.");

            // Lat/Long zorunluluğunu VO içinde veya burada kontrol edebilirsiniz
            if (address.Location == null || address.Location.IsEmpty)
                throw new BranchDomainException("Konum bilgisi (Location) boş olamaz.");

            var branch = GetById(branchId);
            branch.Name = name;
            branch.BrandId = brandId;
            branch.Address = address;
            branch.BranchType = branchType;
            branch.FileId = fileId;

            branch.AddDomainEvent(new BranchUpdatedDomainEvent(branch.Id, branch.BrandId, branch.Name));

            return branch;
        }

        // --- İş Metotları (Business Logic) ---

        public void ChangeAddress(Address newAddress)
        {
            if (newAddress == null)
                throw new BranchDomainException("Yeni adres boş olamaz.");

            if (Address == newAddress)
                return;

            Address = newAddress;

            AddDomainEvent(new BranchAddressUpdatedDomainEvent(Id));
        }

        public void UpdateDetails(string newName, BranchType newType, string? newFileId)
        {
            if (string.IsNullOrWhiteSpace(newName))
                throw new BranchDomainException("Şube adı boş olamaz.");

            Name = newName;
            BranchType = newType;
            FileId = newFileId;

            // Detaylar değiştiğinde de event fırlatılabilir
            AddDomainEvent(new BranchAddressUpdatedDomainEvent(Id));
        }
    }
}