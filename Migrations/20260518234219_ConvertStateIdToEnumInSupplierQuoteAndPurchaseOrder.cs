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
            migrationBuilder.DropForeignKey(
                name: "PurchaseOrders_StateId_fkey",
                table: "PurchaseOrders");

            migrationBuilder.DropForeignKey(
                name: "SupplierQuotes_StateId_fkey",
                table: "SupplierQuotes");

            migrationBuilder.AlterColumn<int>(
                name: "StateId",
                table: "SupplierQuotes",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "SupplierQuotes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "StateId",
                table: "PurchaseOrders",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "PurchaseOrders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_States_StateId",
                table: "PurchaseOrders",
                column: "StateId",
                principalTable: "States",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SupplierQuotes_States_StateId",
                table: "SupplierQuotes",
                column: "StateId",
                principalTable: "States",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_States_StateId",
                table: "PurchaseOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_SupplierQuotes_States_StateId",
                table: "SupplierQuotes");

            migrationBuilder.DropColumn(
                name: "State",
                table: "SupplierQuotes");

            migrationBuilder.DropColumn(
                name: "State",
                table: "PurchaseOrders");

            migrationBuilder.AlterColumn<int>(
                name: "StateId",
                table: "SupplierQuotes",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "StateId",
                table: "PurchaseOrders",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

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
