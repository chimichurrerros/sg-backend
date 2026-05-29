using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class EmployeeRelationsAndPositionHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeKids");

            migrationBuilder.CreateTable(
                name: "EmployeeRelations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeId = table.Column<int>(type: "integer", nullable: false),
                    RelationType = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Lastname = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DocumentNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("EmployeeRelations_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "EmployeeRelations_EmployeeId_fkey",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeRelations_DocumentNumber",
                table: "EmployeeRelations",
                column: "DocumentNumber");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeRelations_EmployeeId_RelationType",
                table: "EmployeeRelations",
                columns: new[] { "EmployeeId", "RelationType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeRelations");

            migrationBuilder.CreateTable(
                name: "EmployeeKids",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeId = table.Column<int>(type: "integer", nullable: false),
                    EntityId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("EmployeeKids_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "EmployeeKids_EmployeeId_fkey",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "EmployeeKids_EntityId_fkey",
                        column: x => x.EntityId,
                        principalTable: "PhysicalPersons",
                        principalColumn: "EntityId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeKids_EmployeeId",
                table: "EmployeeKids",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeKids_EntityId",
                table: "EmployeeKids",
                column: "EntityId");
        }
    }
}
