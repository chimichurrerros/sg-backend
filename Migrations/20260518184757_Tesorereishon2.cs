using BackEnd.Models;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class Tesorereishon2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "PurchaseRequests_StateId_fkey",
                table: "PurchaseRequests");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:account_type_enum", "cash,checking,savings")
                .Annotation("Npgsql:Enum:bank_movement_type_enum", "debit,credit")
                .Annotation("Npgsql:Enum:bill_state_enum", "pending,paid,voided")
                .Annotation("Npgsql:Enum:bill_type_enum", "contado,credito")
                .Annotation("Npgsql:Enum:check_status_enum", "pending,cashed,voided")
                .Annotation("Npgsql:Enum:check_type_enum", "day,deferred")
                .Annotation("Npgsql:Enum:purchase_request_state_enum", "pending,approved,rejected,completed")
                .Annotation("Npgsql:Enum:sales_order_state_enum", "pending,confirmed,cancelled")
                .OldAnnotation("Npgsql:Enum:account_type_enum", "cash,checking,savings")
                .OldAnnotation("Npgsql:Enum:bank_movement_type_enum", "debit,credit")
                .OldAnnotation("Npgsql:Enum:bill_state_enum", "pending,paid,voided")
                .OldAnnotation("Npgsql:Enum:bill_type_enum", "contado,credito")
                .OldAnnotation("Npgsql:Enum:check_status_enum", "pending,cashed,voided")
                .OldAnnotation("Npgsql:Enum:check_type_enum", "day,deferred")
                .OldAnnotation("Npgsql:Enum:sales_order_state_enum", "pending,confirmed,cancelled");

            migrationBuilder.AlterColumn<int>(
                name: "StateId",
                table: "PurchaseRequests",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<PurchaseRequestStateEnum>(
                name: "PurchaseRequestState",
                table: "PurchaseRequests",
                type: "purchase_request_state_enum",
                nullable: false,
                defaultValue: PurchaseRequestStateEnum.Pending);

            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "Bills",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseRequests_States_StateId",
                table: "PurchaseRequests",
                column: "StateId",
                principalTable: "States",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseRequests_States_StateId",
                table: "PurchaseRequests");

            migrationBuilder.DropColumn(
                name: "PurchaseRequestState",
                table: "PurchaseRequests");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "Checks");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:account_type_enum", "cash,checking,savings")
                .Annotation("Npgsql:Enum:bank_movement_type_enum", "debit,credit")
                .Annotation("Npgsql:Enum:bill_state_enum", "pending,paid,voided")
                .Annotation("Npgsql:Enum:bill_type_enum", "contado,credito")
                .Annotation("Npgsql:Enum:check_status_enum", "pending,cashed,voided")
                .Annotation("Npgsql:Enum:check_type_enum", "day,deferred")
                .Annotation("Npgsql:Enum:sales_order_state_enum", "pending,confirmed,cancelled")
                .OldAnnotation("Npgsql:Enum:account_type_enum", "cash,checking,savings")
                .OldAnnotation("Npgsql:Enum:bank_movement_type_enum", "debit,credit")
                .OldAnnotation("Npgsql:Enum:bill_state_enum", "pending,paid,voided")
                .OldAnnotation("Npgsql:Enum:bill_type_enum", "contado,credito")
                .OldAnnotation("Npgsql:Enum:check_status_enum", "pending,cashed,voided")
                .OldAnnotation("Npgsql:Enum:check_type_enum", "day,deferred")
                .OldAnnotation("Npgsql:Enum:purchase_request_state_enum", "pending,approved,rejected,completed")
                .OldAnnotation("Npgsql:Enum:sales_order_state_enum", "pending,confirmed,cancelled");

            migrationBuilder.AlterColumn<int>(
                name: "StateId",
                table: "PurchaseRequests",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "Bills",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "PurchaseRequests_StateId_fkey",
                table: "PurchaseRequests",
                column: "StateId",
                principalTable: "States",
                principalColumn: "Id");
        }
    }
}
