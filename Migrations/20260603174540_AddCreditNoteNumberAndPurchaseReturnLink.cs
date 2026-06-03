using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class AddCreditNoteNumberAndPurchaseReturnLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreditNoteId",
                table: "PurchaseReturns",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Number",
                table: "CreditNotes",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturns_CreditNoteId",
                table: "PurchaseReturns",
                column: "CreditNoteId");

            migrationBuilder.AddForeignKey(
                name: "PurchaseReturns_CreditNoteId_fkey",
                table: "PurchaseReturns",
                column: "CreditNoteId",
                principalTable: "CreditNotes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "PurchaseReturns_CreditNoteId_fkey",
                table: "PurchaseReturns");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseReturns_CreditNoteId",
                table: "PurchaseReturns");

            migrationBuilder.DropColumn(
                name: "CreditNoteId",
                table: "PurchaseReturns");

            migrationBuilder.DropColumn(
                name: "Number",
                table: "CreditNotes");
        }
    }
}
