using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class UwU : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SupplierId",
                table: "PurchaseRequestDetails",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequestDetails_SupplierId",
                table: "PurchaseRequestDetails",
                column: "SupplierId");

            // Delete existing rows that have SupplierId=0 (no valid supplier reference)
            migrationBuilder.Sql(
                "DELETE FROM \"PurchaseRequestDetails\" WHERE \"SupplierId\" = 0");

            migrationBuilder.AddForeignKey(
                name: "PurchaseRequestDetails_SupplierId_fkey",
                table: "PurchaseRequestDetails",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "PurchaseRequestDetails_SupplierId_fkey",
                table: "PurchaseRequestDetails");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseRequestDetails_SupplierId",
                table: "PurchaseRequestDetails");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                table: "PurchaseRequestDetails");
        }
    }
}
