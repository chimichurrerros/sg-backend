using BackEnd.Models;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class ConvertSalesOrderAndBillStateToEnums : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "Bills_StateId_fkey",
                table: "Bills");

            migrationBuilder.DropForeignKey(
                name: "SalesOrders_StateId_fkey",
                table: "SalesOrders");

            migrationBuilder.DropIndex(
                name: "IX_SalesOrders_StateId",
                table: "SalesOrders");

            migrationBuilder.DropIndex(
                name: "IX_Bills_StateId",
                table: "Bills");

            migrationBuilder.DropColumn(
                name: "StateId",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "StateId",
                table: "Bills");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:bank_movement_type_enum", "debit,credit")
                .Annotation("Npgsql:Enum:bill_state_enum", "pending,paid,voided")
                .Annotation("Npgsql:Enum:bill_type_enum", "contado,credito")
                .Annotation("Npgsql:Enum:check_status_enum", "pending,cashed,voided")
                .Annotation("Npgsql:Enum:check_type_enum", "day,deferred")
                .Annotation("Npgsql:Enum:sales_order_state_enum", "pending,confirmed,cancelled")
                .OldAnnotation("Npgsql:Enum:bank_movement_type_enum", "debit,credit")
                .OldAnnotation("Npgsql:Enum:bill_type_enum", "contado,credito")
                .OldAnnotation("Npgsql:Enum:check_status_enum", "pending,cashed,voided")
                .OldAnnotation("Npgsql:Enum:check_type_enum", "day,deferred");

            migrationBuilder.AddColumn<SalesOrderStateEnum>(
                name: "SalesOrderState",
                table: "SalesOrders",
                type: "sales_order_state_enum",
                nullable: false,
                defaultValue: SalesOrderStateEnum.Pending);

            migrationBuilder.AddColumn<BillStateEnum>(
                name: "BillState",
                table: "Bills",
                type: "bill_state_enum",
                nullable: false,
                defaultValue: BillStateEnum.Pending);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SalesOrderState",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "BillState",
                table: "Bills");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:bank_movement_type_enum", "debit,credit")
                .Annotation("Npgsql:Enum:bill_type_enum", "contado,credito")
                .Annotation("Npgsql:Enum:check_status_enum", "pending,cashed,voided")
                .Annotation("Npgsql:Enum:check_type_enum", "day,deferred")
                .OldAnnotation("Npgsql:Enum:bank_movement_type_enum", "debit,credit")
                .OldAnnotation("Npgsql:Enum:bill_state_enum", "pending,paid,voided")
                .OldAnnotation("Npgsql:Enum:bill_type_enum", "contado,credito")
                .OldAnnotation("Npgsql:Enum:check_status_enum", "pending,cashed,voided")
                .OldAnnotation("Npgsql:Enum:check_type_enum", "day,deferred")
                .OldAnnotation("Npgsql:Enum:sales_order_state_enum", "pending,confirmed,cancelled");

            migrationBuilder.AddColumn<int>(
                name: "StateId",
                table: "SalesOrders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StateId",
                table: "Bills",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_StateId",
                table: "SalesOrders",
                column: "StateId");

            migrationBuilder.CreateIndex(
                name: "IX_Bills_StateId",
                table: "Bills",
                column: "StateId");

            migrationBuilder.AddForeignKey(
                name: "Bills_StateId_fkey",
                table: "Bills",
                column: "StateId",
                principalTable: "States",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "SalesOrders_StateId_fkey",
                table: "SalesOrders",
                column: "StateId",
                principalTable: "States",
                principalColumn: "Id");
        }
    }
}
