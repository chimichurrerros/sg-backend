using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestForQuotationIdToSupplierQuote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RequestForQuotationId",
                table: "SupplierQuotes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SupplierQuotes_RequestForQuotationId",
                table: "SupplierQuotes",
                column: "RequestForQuotationId");

            migrationBuilder.AddForeignKey(
                name: "SupplierQuotes_RequestForQuotationId_fkey",
                table: "SupplierQuotes",
                column: "RequestForQuotationId",
                principalTable: "RequestForQuotations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "SupplierQuotes_RequestForQuotationId_fkey",
                table: "SupplierQuotes");

            migrationBuilder.DropIndex(
                name: "IX_SupplierQuotes_RequestForQuotationId",
                table: "SupplierQuotes");

            migrationBuilder.DropColumn(
                name: "RequestForQuotationId",
                table: "SupplierQuotes");
        }
    }
}
