using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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

            builder.HasOne(b => b.Brand)
                .WithMany(br => br.Branches)
                .HasForeignKey(b => b.BrandId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(b => b.ChatRooms)
                .WithOne(c => c.Branch)
                .HasForeignKey(c => c.BranchId);

            builder.HasMany(b => b.Users)
                .WithOne(u => u.Branch)
                .HasForeignKey(u => u.BranchId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
