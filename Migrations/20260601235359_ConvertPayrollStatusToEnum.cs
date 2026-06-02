using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class ConvertPayrollStatusToEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "PayrollProcesses_PayrollStatusId_fkey",
                table: "PayrollProcesses");

            migrationBuilder.DropTable(
                name: "PayrollStatus");

            migrationBuilder.DropIndex(
                name: "IX_PayrollProcesses_PayrollStatusId",
                table: "PayrollProcesses");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PayrollStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PayrollStatus_pkey", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PayrollProcesses_PayrollStatusId",
                table: "PayrollProcesses",
                column: "PayrollStatusId");

            migrationBuilder.AddForeignKey(
                name: "PayrollProcesses_PayrollStatusId_fkey",
                table: "PayrollProcesses",
                column: "PayrollStatusId",
                principalTable: "PayrollStatus",
                principalColumn: "Id");
        }
    }
}
