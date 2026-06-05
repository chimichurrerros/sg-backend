using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchIdToPurchaseRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "PurchaseRequests",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequests_BranchId",
                table: "PurchaseRequests",
                column: "BranchId");

            migrationBuilder.AddForeignKey(
                name: "PurchaseRequests_BranchId_fkey",
                table: "PurchaseRequests",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "PurchaseRequests_BranchId_fkey",
                table: "PurchaseRequests");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseRequests_BranchId",
                table: "PurchaseRequests");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "PurchaseRequests");
        }
    }
}
