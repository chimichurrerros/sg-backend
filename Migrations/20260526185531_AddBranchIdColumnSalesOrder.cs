using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchIdColumnSalesOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "SalesOrders",
                type: "integer",
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_BranchId",
                table: "SalesOrders",
                column: "BranchId");

            migrationBuilder.AddForeignKey(
                name: "SalesOrders_BranchId_fkey",
                table: "SalesOrders",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "SalesOrders_BranchId_fkey",
                table: "SalesOrders");

            migrationBuilder.DropIndex(
                name: "IX_SalesOrders_BranchId",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "SalesOrders");
        }
    }
}
