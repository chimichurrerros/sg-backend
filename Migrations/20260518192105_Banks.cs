using BackEnd.Models;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class Banks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                defaultValue: BankMovementTypeEnum.Debit);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Banks",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "Ruc",
                table: "Banks",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountNumber",
                table: "Banks");

            migrationBuilder.DropColumn(
                name: "AccountType",
                table: "Banks");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Banks");

            migrationBuilder.DropColumn(
                name: "Ruc",
                table: "Banks");
        }
    }
}
