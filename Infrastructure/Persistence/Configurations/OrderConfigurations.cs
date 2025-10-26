using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");
            builder.HasKey(o => o.Id);

            // Address (ValueObject) için Owned Entity Tipi konfigürasyonu
            // Bu, Address'in alanlarını (Street, City, ZipCode)
            // Orders tablosuna kolon olarak ekler.
            builder.OwnsOne(o => o.ShippingAddress, sa =>
            {
                sa.Property(p => p.Street)
                    .HasColumnName("ShippingStreet")
                    .IsRequired()
                    .HasMaxLength(200);

                sa.Property(p => p.City)
                    .HasColumnName("ShippingCity")
                    .IsRequired()
                    .HasMaxLength(100);

                sa.Property(p => p.ZipCode)
                    .HasColumnName("ShippingZipCode")
                    .IsRequired()
                    .HasMaxLength(10);
            });

            // Order ve OrderItem arasındaki 1'e N ilişki
            builder.HasMany(o => o.OrderItems)
                .WithOne() // OrderItem'da Order navigasyonu yok
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Customer ile ilişki
            //builder.HasOne<Customer>()
            //    .WithMany() // Customer'da Order listesi yok
            //    .HasForeignKey(o => o.CustomerId)
            //    .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
