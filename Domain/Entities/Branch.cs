using Domain.Enums;
using Domain.Events.BranchEvents;
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

        private readonly List<Tag> _tags = new(); // <--- DİKKAT: Tag olmalı
        public IReadOnlyCollection<Tag> Tags => _tags.AsReadOnly();

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
            string? fileId = null,
            IEnumerable<string>? tags = null)
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

            if (tags != null && tags.Any())
            {
                // 1. Önce stringleri temizle (boş olanları at)
                // 2. Tag Value Object'ine çevir
                // 3. Tekrarları engelle (Distinct - Tag objesinin Equals metodu sayesinde çalışır)
                var tagObjects = tags
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .Select(t => Tag.Create(t))
                    .Distinct();

                branch._tags.AddRange(tagObjects);
            }

            branch.AddDomainEvent(new BranchCreatedDomainEvent(branch.Id, branch.BrandId, branch.Name));

            return branch;
        }

        public void Update(
            string name,
            Guid brandId,
            Address address,
            BranchType branchType,
            string? fileId,
            IEnumerable<string>? tags = null)
        {
            // Validationlar
            if (string.IsNullOrWhiteSpace(name))
                throw new BranchDomainException("Şube adı (Name) boş olamaz.");

            if (address == null)
                throw new BranchDomainException("Adres bilgisi (Address) boş olamaz.");

            if (address.Location == null || address.Location.IsEmpty)
                throw new BranchDomainException("Konum bilgisi (Location) boş olamaz.");

            if (brandId == Guid.Empty)
                throw new BranchDomainException("Marka (BrandId) boş olamaz.");

            Name = name;
            BrandId = brandId;
            Address = address;
            BranchType = branchType;
            FileId = fileId;

            _tags.Clear();
            if (tags != null && tags.Any())
            {
                // 1. Önce stringleri temizle (boş olanları at)
                // 2. Tag Value Object'ine çevir
                // 3. Tekrarları engelle (Distinct - Tag objesinin Equals metodu sayesinde çalışır)
                var tagObjects = tags
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .Select(t => Tag.Create(t))
                    .Distinct();

                _tags.AddRange(tagObjects);
            }

            // Event fırlat
            AddDomainEvent(new BranchUpdatedDomainEvent(Id, BrandId, Name));
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