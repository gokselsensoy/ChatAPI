using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class ChatRoomConfiguration : IEntityTypeConfiguration<ChatRoom>
    {
        public void Configure(EntityTypeBuilder<ChatRoom> builder)
        {
            builder.ToTable("ChatRooms");
            builder.HasKey(cr => cr.Id);
            builder.Property(cr => cr.Name).IsRequired().HasMaxLength(200);
            builder.Property(cr => cr.RoomType).HasConversion<string>().HasMaxLength(50);

            builder.HasOne(cr => cr.Branch)
                .WithMany(b => b.ChatRooms) // Branch.cs'te tanımlı
                .HasForeignKey(cr => cr.BranchId)
                .OnDelete(DeleteBehavior.Cascade); // Şube silinirse odalar da silinsin
        }
    }
}
