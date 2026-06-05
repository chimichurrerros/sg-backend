using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAccountIdFromPaymentOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
    name: "PaymentOrders_AccountId_fkey",
    table: "PaymentOrders");

migrationBuilder.DropColumn(
    name: "AccountId",
    table: "PaymentOrders");
        }
    }
}
