using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class BranchAdminMapConfiguration : IEntityTypeConfiguration<BranchAdminMap>
    {
        public void Configure(EntityTypeBuilder<BranchAdminMap> builder)
        {
            builder.ToTable("BranchAdminMaps");
            builder.HasKey(m => new { m.BranchId, m.UserId });

            builder.HasOne(m => m.Branch)
                .WithMany(b => b.BranchAdminMaps)
                .HasForeignKey(m => m.BranchId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(m => m.User)
                .WithMany(u => u.BranchAdminMaps)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
