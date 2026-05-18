using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class ConvertStateIdToEnumInSupplierQuoteAndPurchaseOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"PurchaseOrders\" DROP CONSTRAINT IF EXISTS \"PurchaseOrders_StateId_fkey\";");
            migrationBuilder.Sql("ALTER TABLE \"SupplierQuotes\" DROP CONSTRAINT IF EXISTS \"SupplierQuotes_StateId_fkey\";");

            migrationBuilder.RenameColumn(
                name: "StateId",
                table: "SupplierQuotes",
                newName: "State");

            migrationBuilder.RenameColumn(
                name: "StateId",
                table: "PurchaseOrders",
                newName: "State");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "State",
                table: "SupplierQuotes",
                newName: "StateId");

            migrationBuilder.RenameColumn(
                name: "State",
                table: "PurchaseOrders",
                newName: "StateId");

            migrationBuilder.AddForeignKey(
                name: "PurchaseOrders_StateId_fkey",
                table: "PurchaseOrders",
                column: "StateId",
                principalTable: "States",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "SupplierQuotes_StateId_fkey",
                table: "SupplierQuotes",
                column: "StateId",
                principalTable: "States",
                principalColumn: "Id");
        }
    }
}
