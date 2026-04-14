using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ETicaretProjesiV2._0.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSupportEntitiesFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SendAt",
                table: "TicketMessages",
                newName: "UpdatedDate");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "TicketMessages",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "TicketMessages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "SupportTickets",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "SupportTickets",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "TicketMessages");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "TicketMessages");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "SupportTickets");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "SupportTickets");

            migrationBuilder.RenameColumn(
                name: "UpdatedDate",
                table: "TicketMessages",
                newName: "SendAt");
        }
    }
}
