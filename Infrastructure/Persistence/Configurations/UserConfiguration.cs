using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users"); // Bu BİZİM tablomuz
            builder.HasKey(u => u.Id); // Kendi PKey'imiz

            // IdentityId'yi (AspNetUsers.Id) EŞSİZ (UNIQUE) yap
            builder.Property(u => u.IdentityId).IsRequired();
            builder.HasIndex(u => u.IdentityId).IsUnique();

            builder.Property(u => u.UserName).IsRequired().HasMaxLength(100);

            // 1-1 İlişki: Bizim User'ımız, AspNetUsers'taki 1 kayda bağlıdır.
            // Bu, EF Core'un bilmesi için GEREKLİ DEĞİLDİR (çünkü navigasyon eklemiyoruz),
            // ancak IdentityId'nin FK olduğunu bilmek önemlidir.

            // 1-N İlişki: User (1) -> Branch (N)
            builder.HasOne(u => u.Branch)
                .WithMany(b => b.Users)
                .HasForeignKey(u => u.BranchId)
                .OnDelete(DeleteBehavior.SetNull); // Şube silinirse User'ın BranchId'si null olsun
        }
    }
}
