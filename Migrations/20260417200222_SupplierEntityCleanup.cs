using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class SupplierEntityCleanup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "Suppliers_TaxConditionId_fkey",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_Suppliers_TaxConditionId",
                table: "Suppliers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_TaxConditionId",
                table: "Suppliers",
                column: "TaxConditionId");

            migrationBuilder.AddForeignKey(
                name: "Suppliers_TaxConditionId_fkey",
                table: "Suppliers",
                column: "TaxConditionId",
                principalTable: "TaxConditions",
                principalColumn: "Id");
        }
    }
}
