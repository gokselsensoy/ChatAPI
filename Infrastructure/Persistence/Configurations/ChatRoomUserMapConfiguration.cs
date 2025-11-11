using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class ChatRoomUserMapConfiguration : IEntityTypeConfiguration<ChatRoomUserMap>
    {
        public void Configure(EntityTypeBuilder<ChatRoomUserMap> builder)
        {
            builder.ToTable("ChatRoomUserMaps");
            // (ChatRoomId, UserId) composite key olmalı
            builder.HasKey(m => new { m.ChatRoomId, m.UserId });

            builder.HasOne(m => m.ChatRoom)
                .WithMany(cr => cr.ChatRoomUserMaps) // ChatRoom.cs'te tanımlı
                .HasForeignKey(m => m.ChatRoomId)
                .OnDelete(DeleteBehavior.Cascade); // Oda silinirse üyelik de silinsin

            builder.HasOne(m => m.User)
                .WithMany(u => u.ChatRoomMaps) // User.cs'te tanımlı
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Kullanıcı silinirse üyelik de silinsin
        }
    }
}
