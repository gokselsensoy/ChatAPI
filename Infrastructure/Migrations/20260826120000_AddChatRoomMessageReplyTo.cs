using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260826120000_AddChatRoomMessageReplyTo")]
    public partial class AddChatRoomMessageReplyTo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ReplyToMessageId",
                table: "ChatRoomMessages",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatRoomMessages_ReplyToMessageId",
                table: "ChatRoomMessages",
                column: "ReplyToMessageId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRoomMessages_ChatRoomMessages_ReplyToMessageId",
                table: "ChatRoomMessages",
                column: "ReplyToMessageId",
                principalTable: "ChatRoomMessages",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatRoomMessages_ChatRoomMessages_ReplyToMessageId",
                table: "ChatRoomMessages");

            migrationBuilder.DropIndex(
                name: "IX_ChatRoomMessages_ReplyToMessageId",
                table: "ChatRoomMessages");

            migrationBuilder.DropColumn(
                name: "ReplyToMessageId",
                table: "ChatRoomMessages");
        }
    }
}
