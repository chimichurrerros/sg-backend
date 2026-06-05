using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class AddBankAccountAndScheduledPayToPayrollProcess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BankAccountId",
                table: "PayrollProcesses",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledPayDateTime",
                table: "PayrollProcesses",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollProcesses_BankAccountId",
                table: "PayrollProcesses",
                column: "BankAccountId");

            migrationBuilder.AddForeignKey(
                name: "PayrollProcesses_BankAccountId_fkey",
                table: "PayrollProcesses",
                column: "BankAccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "PayrollProcesses_BankAccountId_fkey",
                table: "PayrollProcesses");

            migrationBuilder.DropIndex(
                name: "IX_PayrollProcesses_BankAccountId",
                table: "PayrollProcesses");

            migrationBuilder.DropColumn(
                name: "BankAccountId",
                table: "PayrollProcesses");

            migrationBuilder.DropColumn(
                name: "ScheduledPayDateTime",
                table: "PayrollProcesses");
        }
    }
}
