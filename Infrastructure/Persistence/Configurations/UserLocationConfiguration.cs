using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class UserLocationConfiguration : IEntityTypeConfiguration<UserLocation>
    {
        public void Configure(EntityTypeBuilder<UserLocation> builder)
        {
            builder.ToTable("UserLocations");

            builder.HasKey(x => x.Id);

            // KRİTİK NOKTA: UserId benzersiz olmalı!
            // Bu sayede aynı kullanıcı için 2. satır oluşamaz.
            builder.HasIndex(x => x.UserId)
                .IsUnique();

            builder.Property(x => x.UserId).IsRequired();
            builder.Property(x => x.BranchId).IsRequired();
        }
    }
}
