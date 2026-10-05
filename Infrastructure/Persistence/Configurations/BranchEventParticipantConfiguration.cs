using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class BranchEventParticipantConfiguration : IEntityTypeConfiguration<BranchEventParticipant>
    {
        public void Configure(EntityTypeBuilder<BranchEventParticipant> builder)
        {
            builder.ToTable("BranchEventParticipants");
            builder.HasKey(e => e.Id);

            builder.Property(e => e.HangfireJobId).HasMaxLength(150);

            builder.HasOne(e => e.BranchEvent)
                   .WithMany(e => e.Participants)
                   .HasForeignKey(e => e.BranchEventId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.User)
                   .WithMany()
                   .HasForeignKey(e => e.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Bir kullanıcı aynı etkinliğe birden fazla kez kayıt olamaz
            builder.HasIndex(e => new { e.BranchEventId, e.UserId }).IsUnique();
        }
    }
}