using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class MenuConfiguration : IEntityTypeConfiguration<Menu>
    {
        public void Configure(EntityTypeBuilder<Menu> builder)
        {
            builder.ToTable("Menus");
            builder.HasKey(m => m.Id);

            // KRİTİK NOKTA: Bire-Çok İlişki ve Cascade Delete
            builder.HasMany(m => m.MenuItems)
                   .WithOne(mi => mi.Menu)
                   .HasForeignKey(mi => mi.MenuId)
                   .IsRequired() // MenuId null olamaz!
                   .OnDelete(DeleteBehavior.Cascade); // Menü silinirse veya listeden item çıkarsa DB'den de silinsin.

            // Domain'deki private field'ı EF Core'a tanıtıyoruz (EF Core listeyi buraya dolduracak)
            builder.Metadata.FindNavigation(nameof(Menu.MenuItems))
                   ?.SetPropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
