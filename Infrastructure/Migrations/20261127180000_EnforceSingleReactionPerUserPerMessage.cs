using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261127180000_EnforceSingleReactionPerUserPerMessage")]
    public partial class EnforceSingleReactionPerUserPerMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Eğer bir kullanıcı aynı mesaja birden fazla emoji bırakmışsa,
            // en son bırakılan (veya güncellenen) reaksiyonu tutup eski mükerrer kayıtları temizle.
            migrationBuilder.Sql(@"
                DELETE FROM ""MessageReactions"" a
                USING ""MessageReactions"" b
                WHERE a.""ChatRoomMessageId"" = b.""ChatRoomMessageId""
                  AND a.""UserId"" = b.""UserId""
                  AND (
                      COALESCE(a.""UpdatedDate"", a.""CreatedDate"") < COALESCE(b.""UpdatedDate"", b.""CreatedDate"")
                      OR (
                          COALESCE(a.""UpdatedDate"", a.""CreatedDate"") = COALESCE(b.""UpdatedDate"", b.""CreatedDate"")
                          AND a.""Id"" < b.""Id""
                      )
                  );
            ");

            migrationBuilder.DropIndex(
                name: "IX_MessageReactions_ChatRoomMessageId_UserId_Emoji",
                table: "MessageReactions");

            migrationBuilder.CreateIndex(
                name: "IX_MessageReactions_ChatRoomMessageId_UserId",
                table: "MessageReactions",
                columns: new[] { "ChatRoomMessageId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MessageReactions_ChatRoomMessageId_UserId",
                table: "MessageReactions");

            migrationBuilder.CreateIndex(
                name: "IX_MessageReactions_ChatRoomMessageId_UserId_Emoji",
                table: "MessageReactions",
                columns: new[] { "ChatRoomMessageId", "UserId", "Emoji" },
                unique: true);
        }
    }
}
