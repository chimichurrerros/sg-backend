using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStateToPaymentOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "PaymentOrders_StateId_fkey",
                table: "PaymentOrders");

            migrationBuilder.AlterColumn<int>(
                name: "StateId",
                table: "PaymentOrders",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "PaymentOrders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentOrders_States_StateId",
                table: "PaymentOrders",
                column: "StateId",
                principalTable: "States",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentOrders_States_StateId",
                table: "PaymentOrders");

            migrationBuilder.DropColumn(
                name: "State",
                table: "PaymentOrders");

            migrationBuilder.AlterColumn<int>(
                name: "StateId",
                table: "PaymentOrders",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "PaymentOrders_StateId_fkey",
                table: "PaymentOrders",
                column: "StateId",
                principalTable: "States",
                principalColumn: "Id");
        }
    }
}
