using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class FixPendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ManualConceptIncidents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeId = table.Column<int>(type: "integer", nullable: false),
                    PayrollUpdateId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    OccurrenceDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PayrollProcessId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ManualConceptIncidents_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "ManualConceptIncidents_EmployeeId_fkey",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "ManualConceptIncidents_PayrollProcessId_fkey",
                        column: x => x.PayrollProcessId,
                        principalTable: "PayrollProcesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "ManualConceptIncidents_PayrollUpdateId_fkey",
                        column: x => x.PayrollUpdateId,
                        principalTable: "PayrollUpdates",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ManualConceptIncidents_EmployeeId",
                table: "ManualConceptIncidents",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_ManualConceptIncidents_PayrollProcessId",
                table: "ManualConceptIncidents",
                column: "PayrollProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_ManualConceptIncidents_PayrollUpdateId",
                table: "ManualConceptIncidents",
                column: "PayrollUpdateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ManualConceptIncidents");
        }
    }
}
