using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStockToCheck : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "Accounts_CurrencyId_fkey",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "Bills_EntityId_fkey",
                table: "Bills");

            migrationBuilder.DropForeignKey(
                name: "FK_Stocks_Checks_CheckId",
                table: "Stocks");

            migrationBuilder.DropTable(
                name: "Currencies");

            migrationBuilder.DropIndex(
                name: "IX_Stocks_CheckId",
                table: "Stocks");

            migrationBuilder.DropPrimaryKey(
                name: "Checks_pkey",
                table: "Checks");

            migrationBuilder.DropPrimaryKey(
                name: "Branches_pkey",
                table: "Branches");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_CurrencyId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "CheckId",
                table: "Stocks");

            migrationBuilder.DropColumn(
                name: "CurrencyId",
                table: "Accounts");

            migrationBuilder.RenameColumn(
                name: "EntityId",
                table: "Bills",
                newName: "CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_Bills_EntityId",
                table: "Bills",
                newName: "IX_Bills_CustomerId");

            migrationBuilder.AlterColumn<string>(
                name: "Receiver",
                table: "Checks",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "Checks",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "IssuingBank",
                table: "Checks",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<DateTime>(
                name: "EmisionDate",
                table: "Checks",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Checks",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(15,2)",
                oldPrecision: 15,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Branches",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Branches",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Checks",
                table: "Checks",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Branches",
                table: "Branches",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "Bills_CustomerId_fkey",
                table: "Bills",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "Bills_CustomerId_fkey",
                table: "Bills");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Checks",
                table: "Checks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Branches",
                table: "Branches");

            migrationBuilder.RenameColumn(
                name: "CustomerId",
                table: "Bills",
                newName: "EntityId");

            migrationBuilder.RenameIndex(
                name: "IX_Bills_CustomerId",
                table: "Bills",
                newName: "IX_Bills_EntityId");

            migrationBuilder.AddColumn<int>(
                name: "CheckId",
                table: "Stocks",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Receiver",
                table: "Checks",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "Checks",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "IssuingBank",
                table: "Checks",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EmisionDate",
                table: "Checks",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Checks",
                type: "numeric(15,2)",
                precision: 15,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Branches",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Branches",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "CurrencyId",
                table: "Accounts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "Checks_pkey",
                table: "Checks",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "Branches_pkey",
                table: "Branches",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Currencies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Symbol = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Currencies_pkey", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Stocks_CheckId",
                table: "Stocks",
                column: "CheckId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_CurrencyId",
                table: "Accounts",
                column: "CurrencyId");

            migrationBuilder.AddForeignKey(
                name: "Accounts_CurrencyId_fkey",
                table: "Accounts",
                column: "CurrencyId",
                principalTable: "Currencies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "Bills_EntityId_fkey",
                table: "Bills",
                column: "EntityId",
                principalTable: "Entities",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Stocks_Checks_CheckId",
                table: "Stocks",
                column: "CheckId",
                principalTable: "Checks",
                principalColumn: "Id");
        }
    }
}
