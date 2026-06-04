using BackEnd.Models;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesOrderFieldsToCustomerQuote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccountId",
                table: "CustomerQuotes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<BillTypeEnum>(
                name: "BillType",
                table: "CustomerQuotes",
                type: "bill_type_enum",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "CustomerQuotes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CashierNumber",
                table: "CustomerQuotes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ImportValue",
                table: "CustomerQuotes",
                type: "numeric(15,2)",
                precision: 15,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "MovementType",
                table: "CustomerQuotes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Number",
                table: "CustomerQuotes",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentMethod",
                table: "CustomerQuotes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SaleCondition",
                table: "CustomerQuotes",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerQuotes_BranchId",
                table: "CustomerQuotes",
                column: "BranchId");

            migrationBuilder.AddForeignKey(
                name: "CustomerQuotes_BranchId_fkey",
                table: "CustomerQuotes",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "CustomerQuotes_BranchId_fkey",
                table: "CustomerQuotes");

            migrationBuilder.DropIndex(
                name: "IX_CustomerQuotes_BranchId",
                table: "CustomerQuotes");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "CustomerQuotes");

            migrationBuilder.DropColumn(
                name: "BillType",
                table: "CustomerQuotes");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "CustomerQuotes");

            migrationBuilder.DropColumn(
                name: "CashierNumber",
                table: "CustomerQuotes");

            migrationBuilder.DropColumn(
                name: "ImportValue",
                table: "CustomerQuotes");

            migrationBuilder.DropColumn(
                name: "MovementType",
                table: "CustomerQuotes");

            migrationBuilder.DropColumn(
                name: "Number",
                table: "CustomerQuotes");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "CustomerQuotes");

            migrationBuilder.DropColumn(
                name: "SaleCondition",
                table: "CustomerQuotes");
        }
    }
}
