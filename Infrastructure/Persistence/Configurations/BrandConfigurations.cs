using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class BrandConfigurations : IEntityTypeConfiguration<Brand>
    {
        public void Configure(EntityTypeBuilder<Brand> builder)
        {
            builder.ToTable("Brands");
            builder.HasKey(b => b.Id);

            builder.Property(b => b.Name)
                .HasMaxLength(50)
                .IsRequired();

            builder.HasMany(b => b.Branches)
                .WithOne(br => br.Brand)
                .HasForeignKey(br => br.BrandId)
                .OnDelete(DeleteBehavior.Cascade);

            // Brand (1) -> OwnerUser (1) ilişkisi
            builder.HasOne(b => b.OwnerUser)
                .WithMany() // User'da Brand listesi tutmuyoruz (şimdilik)
                .HasForeignKey(b => b.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict); // Sahibi olan User silinemesin
        }
    }
}
