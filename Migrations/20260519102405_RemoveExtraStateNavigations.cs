using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class RemoveExtraStateNavigations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"CustomerQuotes\" DROP CONSTRAINT IF EXISTS \"FK_CustomerQuotes_States_StateId\";");
            migrationBuilder.Sql("ALTER TABLE \"PurchaseOrders\" DROP CONSTRAINT IF EXISTS \"FK_PurchaseOrders_States_StateId\";");
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_PurchaseOrders_StateId\";");
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_CustomerQuotes_StateId\";");
            migrationBuilder.Sql("ALTER TABLE \"PurchaseOrders\" DROP COLUMN IF EXISTS \"StateId\";");
            migrationBuilder.Sql("ALTER TABLE \"CustomerQuotes\" DROP COLUMN IF EXISTS \"StateId\";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StateId",
                table: "PurchaseOrders",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StateId",
                table: "CustomerQuotes",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_StateId",
                table: "PurchaseOrders",
                column: "StateId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerQuotes_StateId",
                table: "CustomerQuotes",
                column: "StateId");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerQuotes_States_StateId",
                table: "CustomerQuotes",
                column: "StateId",
                principalTable: "States",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_States_StateId",
                table: "PurchaseOrders",
                column: "StateId",
                principalTable: "States",
                principalColumn: "Id");
        }
    }
}
