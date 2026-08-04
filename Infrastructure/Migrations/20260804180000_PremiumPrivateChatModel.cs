using Domain.Enums;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260804180000_PremiumPrivateChatModel")]
    public partial class PremiumPrivateChatModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Mevcut geo'suz 1:1 Private → Premium
            migrationBuilder.Sql("""UPDATE "ChatRooms" SET "RoomType" = 'Premium' WHERE "RoomType" = 'Private';""");

            // Masa / Group → geo'lu Private
            migrationBuilder.Sql("""UPDATE "ChatRooms" SET "RoomType" = 'Private' WHERE "RoomType" = 'Group';""");

            migrationBuilder.AddColumn<string>(
                name: "TargetRoomType",
                table: "ChatRoomInvites",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Premium");

            // Eski davetler Premium kabul edilir (önceki Private davranışı)
            migrationBuilder.Sql("""UPDATE "ChatRoomInvites" SET "TargetRoomType" = 'Premium';""");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSeenAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "LastSeenAt", table: "Users");
            migrationBuilder.DropColumn(name: "TargetRoomType", table: "ChatRoomInvites");

            migrationBuilder.Sql("""UPDATE "ChatRooms" SET "RoomType" = 'Group' WHERE "RoomType" = 'Private';""");
            migrationBuilder.Sql("""UPDATE "ChatRooms" SET "RoomType" = 'Private' WHERE "RoomType" = 'Premium';""");
        }
    }
}
