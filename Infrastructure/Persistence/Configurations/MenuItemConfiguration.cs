using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
    {
        public void Configure(EntityTypeBuilder<MenuItem> builder)
        {
            // 1. Tablo Adı
            builder.ToTable("MenuItems");

            // 2. Primary Key ve ID Üretim Stratejisi (O meşhur hatanın kesin çözümü)
            builder.HasKey(mi => mi.Id);
            builder.Property(mi => mi.Id).ValueGeneratedNever(); // ID'yi Domain katmanımız (Guid.NewGuid) üretecek!

            // 3. Sütun Kısıtlamaları (Validations & Constraints)
            builder.Property(mi => mi.Name)
                   .IsRequired()
                   .HasMaxLength(150); // Güvenlik ve performans için sınır koymak her zaman iyidir

            builder.Property(mi => mi.Description)
                   .HasMaxLength(500);

            // Enum değerini veritabanında integer (sayı) olarak tutmak en performanslısıdır
            builder.Property(mi => mi.CategoryType)
                   .IsRequired()
                   .HasConversion<int>();

            // KRİTİK NOKTA: Para birimi hassasiyeti. (Toplam 18 basamak, 2'si virgülden sonra)
            builder.Property(mi => mi.Price)
                   .IsRequired()
                   .HasColumnType("decimal(18,2)");

            builder.Property(mi => mi.FileId)
                   .HasMaxLength(255); // Resim veya dosya referansı için makul bir sınır

            // 4. Foreign Key (Dış Anahtar) Zorunluluğu
            // (Ana ilişkiyi MenuConfiguration'da kursak da, burada kolonun null olamayacağını garantiye alıyoruz)
            builder.Property(mi => mi.MenuId)
                   .IsRequired();
        }
    }
}
