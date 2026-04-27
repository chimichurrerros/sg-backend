using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class RemoveEntityIdToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "Users_EntityId_fkey",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "EntityId",
                table: "Users",
                newName: "PhysicalPersonEntityId");

            migrationBuilder.RenameIndex(
                name: "IX_Users_EntityId",
                table: "Users",
                newName: "IX_Users_PhysicalPersonEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_PhysicalPersons_PhysicalPersonEntityId",
                table: "Users",
                column: "PhysicalPersonEntityId",
                principalTable: "PhysicalPersons",
                principalColumn: "EntityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_PhysicalPersons_PhysicalPersonEntityId",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "PhysicalPersonEntityId",
                table: "Users",
                newName: "EntityId");

            migrationBuilder.RenameIndex(
                name: "IX_Users_PhysicalPersonEntityId",
                table: "Users",
                newName: "IX_Users_EntityId");

            migrationBuilder.AddForeignKey(
                name: "Users_EntityId_fkey",
                table: "Users",
                column: "EntityId",
                principalTable: "PhysicalPersons",
                principalColumn: "EntityId");
        }
    }
}
