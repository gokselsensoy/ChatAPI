using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class BranchEventConfiguration : IEntityTypeConfiguration<BranchEvent>
    {
        public void Configure(EntityTypeBuilder<BranchEvent> builder)
        {
            builder.ToTable("BranchEvents");
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Title).IsRequired().HasMaxLength(200);
            builder.Property(e => e.Description).HasMaxLength(1000);
            builder.Property(e => e.StartDate).IsRequired();
            builder.Property(e => e.EndDate).IsRequired();

            builder.HasOne(e => e.Branch)
                   .WithMany()
                   .HasForeignKey(e => e.BranchId)
                   .OnDelete(DeleteBehavior.Cascade); // Şube silinirse etkinlik de silinir
        }
    }
}