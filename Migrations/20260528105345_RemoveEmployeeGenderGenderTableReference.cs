using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class RemoveEmployeeGenderGenderTableReference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "Employees_GenderId_fkey",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "PhysicalPersons_GenderId_fkey",
                table: "PhysicalPersons");

            migrationBuilder.DropTable(
                name: "Genders");

            migrationBuilder.DropIndex(
                name: "IX_PhysicalPersons_GenderId",
                table: "PhysicalPersons");

            migrationBuilder.DropIndex(
                name: "IX_Employees_GenderId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "GenderId",
                table: "PhysicalPersons");

            migrationBuilder.RenameColumn(
                name: "GenderId",
                table: "Employees",
                newName: "Gender");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Gender",
                table: "Employees",
                newName: "GenderId");

            migrationBuilder.AddColumn<int>(
                name: "GenderId",
                table: "PhysicalPersons",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Genders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Genders_pkey", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PhysicalPersons_GenderId",
                table: "PhysicalPersons",
                column: "GenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_GenderId",
                table: "Employees",
                column: "GenderId");

            migrationBuilder.AddForeignKey(
                name: "Employees_GenderId_fkey",
                table: "Employees",
                column: "GenderId",
                principalTable: "Genders",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "PhysicalPersons_GenderId_fkey",
                table: "PhysicalPersons",
                column: "GenderId",
                principalTable: "Genders",
                principalColumn: "Id");
        }
    }
}
