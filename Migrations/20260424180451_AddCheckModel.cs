using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "BankMovements_CheckStatusId_fkey",
                table: "BankMovements");

            migrationBuilder.DropTable(
                name: "CheckStatus");

            migrationBuilder.DropIndex(
                name: "IX_BankMovements_CheckStatusId",
                table: "BankMovements");

            migrationBuilder.DropColumn(
                name: "CheckStatusId",
                table: "BankMovements");

            migrationBuilder.AddColumn<int>(
                name: "CheckId",
                table: "Stocks",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Checks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EmisionDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    AvailabilityDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PaymentDate = table.Column<DateOnly>(type: "date", nullable: true),
                    MaturityDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IssuingBank = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Receiver = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Checks_pkey", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Stocks_CheckId",
                table: "Stocks",
                column: "CheckId");

            migrationBuilder.AddForeignKey(
                name: "FK_Stocks_Checks_CheckId",
                table: "Stocks",
                column: "CheckId",
                principalTable: "Checks",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stocks_Checks_CheckId",
                table: "Stocks");

            migrationBuilder.DropTable(
                name: "Checks");

            migrationBuilder.DropIndex(
                name: "IX_Stocks_CheckId",
                table: "Stocks");

            migrationBuilder.DropColumn(
                name: "CheckId",
                table: "Stocks");

            migrationBuilder.AddColumn<int>(
                name: "CheckStatusId",
                table: "BankMovements",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CheckStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("CheckStatus_pkey", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BankMovements_CheckStatusId",
                table: "BankMovements",
                column: "CheckStatusId");

            migrationBuilder.AddForeignKey(
                name: "BankMovements_CheckStatusId_fkey",
                table: "BankMovements",
                column: "CheckStatusId",
                principalTable: "CheckStatus",
                principalColumn: "Id");
        }
    }
}
