using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class RefactorCustomerQuoteWithStatusEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "CustomerQuotes_StateId_fkey",
                table: "CustomerQuotes");

            migrationBuilder.DropColumn(
                name: "TaxRate",
                table: "CustomerQuoteDetails");

            migrationBuilder.AlterColumn<int>(
                name: "StateId",
                table: "CustomerQuotes",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "CustomerQuotes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerQuotes_States_StateId",
                table: "CustomerQuotes",
                column: "StateId",
                principalTable: "States",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerQuotes_States_StateId",
                table: "CustomerQuotes");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "CustomerQuotes");

            migrationBuilder.AlterColumn<int>(
                name: "StateId",
                table: "CustomerQuotes",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxRate",
                table: "CustomerQuoteDetails",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddForeignKey(
                name: "CustomerQuotes_StateId_fkey",
                table: "CustomerQuotes",
                column: "StateId",
                principalTable: "States",
                principalColumn: "Id");
        }
    }
}
