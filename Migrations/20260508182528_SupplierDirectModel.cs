using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class SupplierDirectModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "Suppliers_EntityId_fkey",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_Suppliers_EntityId",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "EntityId",
                table: "Suppliers");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Suppliers",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BussinessName",
                table: "Suppliers",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Suppliers",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FantasyName",
                table: "Suppliers",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Suppliers",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Suppliers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ruc",
                table: "Suppliers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "Suppliers_Ruc_key",
                table: "Suppliers",
                column: "Ruc",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "Suppliers_Ruc_key",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "BussinessName",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "FantasyName",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "Ruc",
                table: "Suppliers");

            migrationBuilder.AddColumn<int>(
                name: "EntityId",
                table: "Suppliers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_EntityId",
                table: "Suppliers",
                column: "EntityId");

            migrationBuilder.AddForeignKey(
                name: "Suppliers_EntityId_fkey",
                table: "Suppliers",
                column: "EntityId",
                principalTable: "Entities",
                principalColumn: "Id");
        }
    }
}
