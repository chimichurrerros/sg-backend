using BackEnd.Models;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountantProcessStateEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "AccountantProcesses_StateId_fkey",
                table: "AccountantProcesses");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:account_type_enum", "cash,checking,savings")
                .Annotation("Npgsql:Enum:accountant_process_state", "open,close")
                .Annotation("Npgsql:Enum:accountant_process_state_enum", "close,open")
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
                .OldAnnotation("Npgsql:Enum:purchase_request_state_enum", "pending,approved,rejected,completed")
                .OldAnnotation("Npgsql:Enum:sales_order_state_enum", "pending,confirmed,cancelled");

            migrationBuilder.AlterColumn<int>(
                name: "StateId",
                table: "AccountantProcesses",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_AccountantProcesses_States_StateId",
                table: "AccountantProcesses",
                column: "StateId",
                principalTable: "States",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccountantProcesses_States_StateId",
                table: "AccountantProcesses");

            migrationBuilder.DropColumn(
                name: "State",
                table: "AccountantProcesses");

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
                .OldAnnotation("Npgsql:Enum:accountant_process_state", "open,close")
                .OldAnnotation("Npgsql:Enum:accountant_process_state_enum", "close,open")
                .OldAnnotation("Npgsql:Enum:bank_movement_type_enum", "debit,credit")
                .OldAnnotation("Npgsql:Enum:bill_state_enum", "pending,paid,voided")
                .OldAnnotation("Npgsql:Enum:bill_type_enum", "contado,credito")
                .OldAnnotation("Npgsql:Enum:check_status_enum", "pending,cashed,voided")
                .OldAnnotation("Npgsql:Enum:check_type_enum", "day,deferred")
                .OldAnnotation("Npgsql:Enum:purchase_request_state_enum", "pending,approved,rejected,completed")
                .OldAnnotation("Npgsql:Enum:sales_order_state_enum", "pending,confirmed,cancelled");

            migrationBuilder.AlterColumn<int>(
                name: "StateId",
                table: "AccountantProcesses",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "AccountantProcesses_StateId_fkey",
                table: "AccountantProcesses",
                column: "StateId",
                principalTable: "States",
                principalColumn: "Id");
        }
    }
}
