using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class SyncPurchaseRequestStateModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"PurchaseRequestDetails\" DROP CONSTRAINT IF EXISTS \"PurchaseRequestDetails_SupplierId_fkey\";");

            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_PurchaseRequests_StateId\";");

            migrationBuilder.DropColumn(
                name: "StateId",
                table: "PurchaseRequests");

            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "Bills",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StateId",
                table: "PurchaseRequests",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "Bills",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequests_StateId",
                table: "PurchaseRequests",
                column: "StateId");
        }
    }
}
