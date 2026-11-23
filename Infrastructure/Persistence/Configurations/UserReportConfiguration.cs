using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class UserReportConfiguration : IEntityTypeConfiguration<UserReport>
    {
        public void Configure(EntityTypeBuilder<UserReport> builder)
        {
            builder.ToTable("UserReports");
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Reason)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(r => r.Description)
                .HasMaxLength(500);

            builder.HasOne(r => r.ReporterUser)
                .WithMany()
                .HasForeignKey(r => r.ReporterUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.ReportedUser)
                .WithMany()
                .HasForeignKey(r => r.ReportedUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.ChatRoom)
                .WithMany()
                .HasForeignKey(r => r.ChatRoomId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(r => r.Message)
                .WithMany()
                .HasForeignKey(r => r.MessageId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
