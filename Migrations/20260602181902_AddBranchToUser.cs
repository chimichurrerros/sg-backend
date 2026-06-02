using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "Users",
                type: "integer",
                nullable: true);

            // Asignar el ID de la primera sucursal disponible a los usuarios existentes
            migrationBuilder.Sql("UPDATE \"Users\" SET \"BranchId\" = (SELECT \"Id\" FROM \"Branches\" LIMIT 1) WHERE \"BranchId\" IS NULL");

            // Si hay usuarios y no había sucursales, asegurar que no falle la restricción NOT NULL
            // aunque asumimos que existe al menos una sucursal en la base de datos de desarrollo.
            migrationBuilder.AlterColumn<int>(
                name: "BranchId",
                table: "Users",
                type: "integer",
                nullable: false,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_BranchId",
                table: "Users",
                column: "BranchId");

            migrationBuilder.AddForeignKey(
                name: "Users_BranchId_fkey",
                table: "Users",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "Users_BranchId_fkey",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_BranchId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Users");
        }
    }
}
