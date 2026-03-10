using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace Infrastructure.Persistence.Configurations
{
    public class BranchConfiguration : IEntityTypeConfiguration<Branch>
    {
        public void Configure(EntityTypeBuilder<Branch> builder)
        {
            builder.ToTable("Branches");
            builder.HasKey(b => b.Id);

            builder.Property(b => b.Name).HasMaxLength(200).IsRequired();

            builder.Property(b => b.BranchType)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.OwnsOne(b => b.Address, address =>
            {
                address.Property(p => p.Country).HasColumnName("Address_Country").HasMaxLength(100);
                address.Property(p => p.City).HasColumnName("Address_City").HasMaxLength(100);
                address.Property(p => p.District).HasColumnName("Address_District").HasMaxLength(100);
                address.Property(p => p.Neighborhood).HasColumnName("Address_Neighborhood").HasMaxLength(200);
                address.Property(p => p.Street).HasColumnName("Address_Street").HasMaxLength(250);
                address.Property(p => p.BuildingNumber).HasColumnName("Address_BuildingNumber").HasMaxLength(20);
                address.Property(p => p.ApartmentNumber).HasColumnName("Address_ApartmentNumber").HasMaxLength(20); // nullable
                address.Property(p => p.ZipCode).HasColumnName("Address_ZipCode").HasMaxLength(20);

                address.Property(p => p.Location)
                        .HasColumnName("Address_Location")
                        .HasColumnType("geometry(Point)");
            });

            var tagConverter = new ValueConverter<IReadOnlyCollection<Tag>, string>(
                // 1. C# -> DB (Yazarken sorun yok)
                v => JsonSerializer.Serialize(v.Select(t => t.Value).ToList(), (JsonSerializerOptions)null),

                // 2. DB -> C# (OKURKEN GÜVENLİK EKLENDİ)
                v => string.IsNullOrWhiteSpace(v)
                        ? new List<Tag>() // EĞER DB'DEN GELEN VERİ BOŞSA, PATLAMA, DİREKT BOŞ LİSTE DÖN!
                        : (JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null) ?? new List<string>())
                            .Select(s => Tag.Create(s)).ToList()
            );

            // 2. Comparer'ı ayrı bir nesne olarak tanımlıyoruz
            var tagComparer = new ValueComparer<IReadOnlyCollection<Tag>>(
                (c1, c2) => c1.SequenceEqual(c2),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList().AsReadOnly()
            );

            // 3. Property'ye bu hazır nesneleri veriyoruz
            builder.Property(b => b.Tags)
                .HasConversion(tagConverter) // <--- ARTIK HATA VERMEYECEK
                .Metadata.SetValueComparer(tagComparer);

            builder.Metadata.FindNavigation(nameof(Branch.Tags))
                ?.SetPropertyAccessMode(PropertyAccessMode.Field);

            builder.HasOne(b => b.Brand)
                .WithMany(br => br.Branches)
                .HasForeignKey(b => b.BrandId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(b => b.ChatRooms)
                .WithOne(c => c.Branch)
                .HasForeignKey(c => c.BranchId);
        }
    }
}
