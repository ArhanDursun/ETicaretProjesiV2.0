using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ETicaretProjesiV2._0.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MakeAnswerContentNullable12 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ProductQuestions_ProductId",
                table: "ProductQuestions",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductComments_ProductId",
                table: "ProductComments",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductComments_Products_ProductId",
                table: "ProductComments",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductQuestions_Products_ProductId",
                table: "ProductQuestions",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductComments_Products_ProductId",
                table: "ProductComments");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductQuestions_Products_ProductId",
                table: "ProductQuestions");

            migrationBuilder.DropIndex(
                name: "IX_ProductQuestions_ProductId",
                table: "ProductQuestions");

            migrationBuilder.DropIndex(
                name: "IX_ProductComments_ProductId",
                table: "ProductComments");
        }
    }
}
