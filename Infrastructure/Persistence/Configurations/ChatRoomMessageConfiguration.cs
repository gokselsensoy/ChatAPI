using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class ChatRoomMessageConfiguration : IEntityTypeConfiguration<ChatRoomMessage>
    {
        public void Configure(EntityTypeBuilder<ChatRoomMessage> builder)
        {
            builder.ToTable("ChatRoomMessages");
            builder.HasKey(m => m.Id);
            builder.Property(m => m.Message).IsRequired();

            builder.HasOne(m => m.ChatRoom)
                .WithMany(cr => cr.Messages) // ChatRoom.cs'te tanımlı
                .HasForeignKey(m => m.ChatRoomId)
                .OnDelete(DeleteBehavior.Cascade); // Oda silinirse mesajlar da silinsin

            builder.HasOne(m => m.SenderUser)
                .WithMany(u => u.SentMessages) // User.cs'te tanımlı
                .HasForeignKey(m => m.SenderUserId)
                .OnDelete(DeleteBehavior.Restrict); // Kullanıcı silinirse mesajları kalsın
        }
    }
}
