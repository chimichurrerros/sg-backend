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

            migrationBuilder.AddColumn<PurchaseRequestStateEnum>(
                name: "PurchaseRequestState",
                table: "PurchaseRequests",
                type: "purchase_request_state_enum",
                nullable: false,
                defaultValue: PurchaseRequestStateEnum.Pending);

            migrationBuilder.Sql(@"
                UPDATE ""PurchaseRequests"" pr
                SET ""PurchaseRequestState"" = CASE LOWER(s.""Name"")
                    WHEN 'pending' THEN 'pending'::purchase_request_state_enum
                    WHEN 'approved' THEN 'approved'::purchase_request_state_enum
                    WHEN 'rejected' THEN 'rejected'::purchase_request_state_enum
                    WHEN 'completed' THEN 'completed'::purchase_request_state_enum
                    ELSE 'pending'::purchase_request_state_enum
                END
                FROM ""States"" s
                WHERE pr.""StateId"" = s.""Id"";");

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
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE ""PurchaseRequests"" pr
                SET ""StateId"" = s.""Id""
                FROM ""States"" s
                WHERE LOWER(s.""Name"") = CASE pr.""PurchaseRequestState""
                    WHEN 0 THEN 'pending'
                    WHEN 1 THEN 'approved'
                    WHEN 2 THEN 'rejected'
                    WHEN 3 THEN 'completed'
                    ELSE 'pending'
                END;");

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
                name: "CustomerId",
                table: "Bills",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "StateId",
                table: "PurchaseRequests",
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
