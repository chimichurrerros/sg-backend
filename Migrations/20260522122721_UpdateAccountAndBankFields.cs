using BackEnd.Models;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAccountAndBankFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountNumber",
                table: "Banks");

            migrationBuilder.DropColumn(
                name: "AccountType",
                table: "Banks");

            migrationBuilder.AddColumn<string>(
                name: "AccountNumber",
                table: "Accounts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Accounts",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountNumber",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Accounts");

            migrationBuilder.AddColumn<string>(
                name: "AccountNumber",
                table: "Banks",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<BankMovementTypeEnum>(
                name: "AccountType",
                table: "Banks",
                type: "bank_movement_type_enum",
                nullable: false,
                defaultValue: (BankMovementTypeEnum)0);
        }
    }
}
