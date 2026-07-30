using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class UserDeviceTokenConfiguration : IEntityTypeConfiguration<UserDeviceToken>
    {
        public void Configure(EntityTypeBuilder<UserDeviceToken> builder)
        {
            builder.ToTable("UserDeviceTokens");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Token).IsRequired().HasMaxLength(512);
            builder.Property(x => x.Platform).IsRequired().HasMaxLength(20);

            builder.HasIndex(x => x.Token).IsUnique();
            builder.HasIndex(x => x.UserId);

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
