using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ETicaretProjesiV2._0.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProductCommentAndQuestion1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Comment",
                table: "ProductComments",
                newName: "Content");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ProductQuestions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "ProductQuestions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ProductComments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "ProductComments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ProductQuestions");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "ProductQuestions");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ProductComments");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "ProductComments");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "ProductComments",
                newName: "Comment");
        }
    }
}
