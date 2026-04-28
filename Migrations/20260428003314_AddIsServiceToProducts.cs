using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class AddIsServiceToProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"Users\" DROP CONSTRAINT IF EXISTS \"FK_Users_PhysicalPersons_PhysicalPersonEntityId\";");
            migrationBuilder.Sql("ALTER TABLE \"Users\" DROP CONSTRAINT IF EXISTS \"Users_EntityId_fkey\";");
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_Users_PhysicalPersonEntityId\";");
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_Users_EntityId\";");
            migrationBuilder.Sql("ALTER TABLE \"Users\" DROP COLUMN IF EXISTS \"PhysicalPersonEntityId\";");
            migrationBuilder.Sql("ALTER TABLE \"Users\" DROP COLUMN IF EXISTS \"EntityId\";");

            migrationBuilder.AlterColumn<int>(
                name: "ProductCategoryId",
                table: "Products",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "ProductBrandId",
                table: "Products",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<decimal>(
                name: "MinimumStock",
                table: "Products",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AddColumn<bool>(
                name: "IsService",
                table: "Products",
                type: "boolean",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsService",
                table: "Products");

            migrationBuilder.AddColumn<int>(
                name: "PhysicalPersonEntityId",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ProductCategoryId",
                table: "Products",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ProductBrandId",
                table: "Products",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "MinimumStock",
                table: "Products",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2,
                oldNullable: true);

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
