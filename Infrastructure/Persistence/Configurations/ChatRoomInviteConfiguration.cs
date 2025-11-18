using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class ChatRoomInviteConfiguration : IEntityTypeConfiguration<ChatRoomInvite>
    {
        public void Configure(EntityTypeBuilder<ChatRoomInvite> builder)
        {
            builder.ToTable("ChatRoomInvites");
            builder.HasKey(i => i.Id);

            builder.Property(i => i.Status)
                .HasConversion<string>() // Enum'ı string olarak tut
                .HasMaxLength(50);

            // İlişki 1: Davet hangi ODA için?
            builder.HasOne(i => i.ChatRoom)
                .WithMany(c => c.ChatRoomInvites) // ChatRoom.cs'teki koleksiyon
                .HasForeignKey(i => i.ChatRoomId)
                .OnDelete(DeleteBehavior.Cascade); // Oda silinirse davetler de silinsin

            // İlişki 2: Daveti KİM gönderdi? (Inviter)
            builder.HasOne(i => i.InviterUser)
                .WithMany(u => u.SentInvites) // User.cs'teki 'SentInvites' koleksiyonu
                .HasForeignKey(i => i.InviterUserId)
                .OnDelete(DeleteBehavior.Restrict); // Kullanıcı silinirse davetler kalabilir veya silinemez (Restrict güvenlidir)

            // İlişki 3: Davet KİME gönderildi? (Invitee)
            // HATAYI ÇÖZEN KISIM BURASI:
            builder.HasOne(i => i.InviteeUser)
                .WithMany(u => u.ReceivedInvites) // User.cs'teki 'ReceivedInvites' koleksiyonu
                .HasForeignKey(i => i.InviteeUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
