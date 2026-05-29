using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations;

public partial class ConvertPayrollCatalogsToEnums : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "PayrollProcesses_ProcessTypeId_fkey",
            table: "PayrollProcesses");

        migrationBuilder.DropForeignKey(
            name: "PayrollUpdates_FormulaTypeId_fkey",
            table: "PayrollUpdates");

        migrationBuilder.DropForeignKey(
            name: "PayrollUpdates_PayrollTypeId_fkey",
            table: "PayrollUpdates");

        migrationBuilder.DropTable(
            name: "ProcessTypes");

        migrationBuilder.DropTable(
            name: "FormulaTypes");

        migrationBuilder.DropTable(
            name: "PayrollTypes");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "FormulaTypes",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("FormulaTypes_pkey", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "PayrollTypes",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PayrollTypes_pkey", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "ProcessTypes",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("ProcessTypes_pkey", x => x.Id);
            });

        migrationBuilder.AddForeignKey(
            name: "PayrollProcesses_ProcessTypeId_fkey",
            table: "PayrollProcesses",
            column: "ProcessTypeId",
            principalTable: "ProcessTypes",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "PayrollUpdates_FormulaTypeId_fkey",
            table: "PayrollUpdates",
            column: "FormulaTypeId",
            principalTable: "FormulaTypes",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "PayrollUpdates_PayrollTypeId_fkey",
            table: "PayrollUpdates",
            column: "PayrollTypeId",
            principalTable: "PayrollTypes",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }
}