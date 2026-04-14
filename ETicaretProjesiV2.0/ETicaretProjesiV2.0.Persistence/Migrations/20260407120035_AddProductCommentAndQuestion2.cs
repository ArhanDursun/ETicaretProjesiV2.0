using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ETicaretProjesiV2._0.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProductCommentAndQuestion2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ProductQuestions_AppUserId",
                table: "ProductQuestions",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductComments_AppUserId",
                table: "ProductComments",
                column: "AppUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductComments_AspNetUsers_AppUserId",
                table: "ProductComments",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductQuestions_AspNetUsers_AppUserId",
                table: "ProductQuestions",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductComments_AspNetUsers_AppUserId",
                table: "ProductComments");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductQuestions_AspNetUsers_AppUserId",
                table: "ProductQuestions");

            migrationBuilder.DropIndex(
                name: "IX_ProductQuestions_AppUserId",
                table: "ProductQuestions");

            migrationBuilder.DropIndex(
                name: "IX_ProductComments_AppUserId",
                table: "ProductComments");
        }
    }
}
