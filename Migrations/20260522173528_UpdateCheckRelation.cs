using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCheckRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BankMovementId",
                table: "Checks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ConciliationDate",
                table: "Checks",
                type: "date",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Ruc",
                table: "Banks",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_Checks_BankMovementId",
                table: "Checks",
                column: "BankMovementId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Checks_BankMovements_BankMovementId",
                table: "Checks",
                column: "BankMovementId",
                principalTable: "BankMovements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Checks_BankMovements_BankMovementId",
                table: "Checks");

            migrationBuilder.DropIndex(
                name: "IX_Checks_BankMovementId",
                table: "Checks");

            migrationBuilder.DropColumn(
                name: "BankMovementId",
                table: "Checks");

            migrationBuilder.DropColumn(
                name: "ConciliationDate",
                table: "Checks");

            migrationBuilder.AlterColumn<string>(
                name: "Ruc",
                table: "Banks",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
