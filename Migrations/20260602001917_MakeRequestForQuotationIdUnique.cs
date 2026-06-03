using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class MakeRequestForQuotationIdUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SupplierQuotes_RequestForQuotationId",
                table: "SupplierQuotes");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierQuotes_RequestForQuotationId_Unique",
                table: "SupplierQuotes",
                column: "RequestForQuotationId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SupplierQuotes_RequestForQuotationId_Unique",
                table: "SupplierQuotes");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierQuotes_RequestForQuotationId",
                table: "SupplierQuotes",
                column: "RequestForQuotationId");
        }
    }
}
