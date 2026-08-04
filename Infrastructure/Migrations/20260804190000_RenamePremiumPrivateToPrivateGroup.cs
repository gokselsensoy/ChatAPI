using Domain.Enums;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <summary>
    /// Premium → Private (1:1 geo'suz), önceki Private → Group (geo'lu).
    /// 20260804180000 uygulandıysa çalıştır. Henüz uygulanmadıysa 180000 sonrası bu migration da gerekir.
    /// </summary>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260804190000_RenamePremiumPrivateToPrivateGroup")]
    public partial class RenamePremiumPrivateToPrivateGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Sıra önemli: önce geo'lu Private → Group, sonra Premium → Private
            migrationBuilder.Sql("""UPDATE "ChatRooms" SET "RoomType" = 'Group' WHERE "RoomType" = 'Private';""");
            migrationBuilder.Sql("""UPDATE "ChatRooms" SET "RoomType" = 'Private' WHERE "RoomType" = 'Premium';""");

            migrationBuilder.Sql("""UPDATE "ChatRoomInvites" SET "TargetRoomType" = 'Group' WHERE "TargetRoomType" = 'Private';""");
            migrationBuilder.Sql("""UPDATE "ChatRoomInvites" SET "TargetRoomType" = 'Private' WHERE "TargetRoomType" = 'Premium';""");

            migrationBuilder.AlterColumn<string>(
                name: "TargetRoomType",
                table: "ChatRoomInvites",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: nameof(RoomType.Private));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""UPDATE "ChatRooms" SET "RoomType" = 'Premium' WHERE "RoomType" = 'Private';""");
            migrationBuilder.Sql("""UPDATE "ChatRooms" SET "RoomType" = 'Private' WHERE "RoomType" = 'Group';""");

            migrationBuilder.Sql("""UPDATE "ChatRoomInvites" SET "TargetRoomType" = 'Premium' WHERE "TargetRoomType" = 'Private';""");
            migrationBuilder.Sql("""UPDATE "ChatRoomInvites" SET "TargetRoomType" = 'Private' WHERE "TargetRoomType" = 'Group';""");

            migrationBuilder.AlterColumn<string>(
                name: "TargetRoomType",
                table: "ChatRoomInvites",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Premium");
        }
    }
}
