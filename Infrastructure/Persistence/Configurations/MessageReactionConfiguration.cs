using Domain.Entities;
using Domain.SeedWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class MessageReactionConfiguration : IEntityTypeConfiguration<MessageReaction>
    {
        public void Configure(EntityTypeBuilder<MessageReaction> builder)
        {
            builder.ToTable("MessageReactions");

            //Biz ID'leri hep GUID ile manuel (Create Modunda) ürettiğimiz için EF'ye kedni üretmemesini söylüyoruz
            builder.Property(x => x.Id).ValueGeneratedNever();
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Emoji).IsRequired().HasMaxLength(10);

            // 1. İlişki: Mesaj - Reaksiyon (Mesaj silinirse, ona atılan reaksiyonlar silinsin)
            builder.HasOne(x => x.ChatRoomMessage)
                .WithMany(x => x.Reactions)
                .HasForeignKey(x => x.ChatRoomMessageId)
                .OnDelete(DeleteBehavior.Cascade);

            // 2. İlişki Kullanıcı - Reaksiyon (Kullanıcı silinsede attığı tepkiler kalsın)
            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Çok Önemli Mirari Kural: Aynı mesaja, aynı kullanıcı, aynı emojiden sadece 1 tane atabilsin!
            // (Veritabanı seviyesinde koruma kalkanı)
            builder.HasIndex(x => new { x.ChatRoomMessageId, x.UserId, x.Emoji }).IsUnique();
        }
    }
}