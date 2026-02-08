using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class CheckInHistoryConfiguration : IEntityTypeConfiguration<CheckInHistory>
    {
        public void Configure(EntityTypeBuilder<CheckInHistory> builder)
        {
            builder.ToTable("CheckInHistories");

            builder.HasKey(x => x.Id);

            // Burada UserId unique DEĞİL, çünkü log tablosu.
            // Hızlı sorgulama için normal index koyuyoruz.
            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => x.BranchId);
        }
    }
}
