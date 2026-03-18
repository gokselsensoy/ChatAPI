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

            // BUNU BURAYA DA EKLİYORUZ:
            builder.Property(m => m.Id).ValueGeneratedNever();

            // Bire-Çok İlişki ve Field Mapping ayarların (Aynen kalıyor)
            builder.HasMany(m => m.MenuItems)
                   .WithOne(mi => mi.Menu)
                   .HasForeignKey(mi => mi.MenuId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Metadata.FindNavigation(nameof(Menu.MenuItems))
                   ?.SetPropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
