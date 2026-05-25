using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class EmployeeMaritalStatusEnumAndActiveSpouseIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "PhysicalPersons_MaritalStatusId_fkey",
                table: "PhysicalPersons");

            migrationBuilder.DropTable(
                name: "MaritalStatus");

            migrationBuilder.DropIndex(
                name: "IX_PhysicalPersons_MaritalStatusId",
                table: "PhysicalPersons");

            migrationBuilder.DropColumn(
                name: "MaritalStatusId",
                table: "PhysicalPersons");

            migrationBuilder.AddColumn<int>(
                name: "MaritalStatus",
                table: "Employees",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeRelations_OneActiveSpouse",
                table: "EmployeeRelations",
                column: "EmployeeId",
                unique: true,
                filter: "\"RelationType\" = 1 AND \"EndDate\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmployeeRelations_OneActiveSpouse",
                table: "EmployeeRelations");

            migrationBuilder.DropColumn(
                name: "MaritalStatus",
                table: "Employees");

            migrationBuilder.AddColumn<int>(
                name: "MaritalStatusId",
                table: "PhysicalPersons",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "MaritalStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("MaritalStatus_pkey", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PhysicalPersons_MaritalStatusId",
                table: "PhysicalPersons",
                column: "MaritalStatusId");

            migrationBuilder.AddForeignKey(
                name: "PhysicalPersons_MaritalStatusId_fkey",
                table: "PhysicalPersons",
                column: "MaritalStatusId",
                principalTable: "MaritalStatus",
                principalColumn: "Id");
        }
    }
}
