using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseOrderRelationAndLineSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PurchaseRequestId",
                table: "PurchaseOrders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SupplierQuoteDetailId",
                table: "PurchaseOrderDetails",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_PurchaseRequestId",
                table: "PurchaseOrders",
                column: "PurchaseRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderDetails_SupplierQuoteDetailId",
                table: "PurchaseOrderDetails",
                column: "SupplierQuoteDetailId");

            migrationBuilder.AddForeignKey(
                name: "PurchaseOrderDetails_SupplierQuoteDetailId_fkey",
                table: "PurchaseOrderDetails",
                column: "SupplierQuoteDetailId",
                principalTable: "SupplierQuoteDetails",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "PurchaseOrders_PurchaseRequestId_fkey",
                table: "PurchaseOrders",
                column: "PurchaseRequestId",
                principalTable: "PurchaseRequests",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "PurchaseOrderDetails_SupplierQuoteDetailId_fkey",
                table: "PurchaseOrderDetails");

            migrationBuilder.DropForeignKey(
                name: "PurchaseOrders_PurchaseRequestId_fkey",
                table: "PurchaseOrders");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrders_PurchaseRequestId",
                table: "PurchaseOrders");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrderDetails_SupplierQuoteDetailId",
                table: "PurchaseOrderDetails");

            migrationBuilder.DropColumn(
                name: "PurchaseRequestId",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "SupplierQuoteDetailId",
                table: "PurchaseOrderDetails");
        }
    }
}
