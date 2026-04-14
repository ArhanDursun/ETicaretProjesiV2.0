using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ETicaretProjesiV2._0.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameReceiverColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DirectMessages_AspNetUsers_RecieverId",
                table: "DirectMessages");

            migrationBuilder.RenameColumn(
                name: "RecieverId",
                table: "DirectMessages",
                newName: "ReceiverId");

            migrationBuilder.RenameIndex(
                name: "IX_DirectMessages_RecieverId",
                table: "DirectMessages",
                newName: "IX_DirectMessages_ReceiverId");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "DirectMessages",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "DirectMessages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "DirectMessages",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddForeignKey(
                name: "FK_DirectMessages_AspNetUsers_ReceiverId",
                table: "DirectMessages",
                column: "ReceiverId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DirectMessages_AspNetUsers_ReceiverId",
                table: "DirectMessages");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "DirectMessages");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "DirectMessages");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "DirectMessages");

            migrationBuilder.RenameColumn(
                name: "ReceiverId",
                table: "DirectMessages",
                newName: "RecieverId");

            migrationBuilder.RenameIndex(
                name: "IX_DirectMessages_ReceiverId",
                table: "DirectMessages",
                newName: "IX_DirectMessages_RecieverId");

            migrationBuilder.AddForeignKey(
                name: "FK_DirectMessages_AspNetUsers_RecieverId",
                table: "DirectMessages",
                column: "RecieverId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
