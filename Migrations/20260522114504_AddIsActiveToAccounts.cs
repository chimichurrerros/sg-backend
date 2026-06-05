using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveToAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_PhysicalPersons_PhysicalPersonEntityId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_PhysicalPersonEntityId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PhysicalPersonEntityId",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "AccountNumber",
                table: "Accounts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Accounts",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountNumber",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Accounts");

            migrationBuilder.AddColumn<int>(
                name: "PhysicalPersonEntityId",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_PhysicalPersonEntityId",
                table: "Users",
                column: "PhysicalPersonEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_PhysicalPersons_PhysicalPersonEntityId",
                table: "Users",
                column: "PhysicalPersonEntityId",
                principalTable: "PhysicalPersons",
                principalColumn: "EntityId");
        }
    }
}
