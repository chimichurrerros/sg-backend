using BackEnd.Models;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class AddModuleEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "Entries_ModuleId_fkey",
                table: "Entries");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:account_type_enum", "cash,checking,savings")
                .Annotation("Npgsql:Enum:bank_movement_type_enum", "debit,credit")
                .Annotation("Npgsql:Enum:bill_state_enum", "pending,paid,voided")
                .Annotation("Npgsql:Enum:bill_type_enum", "contado,credito")
                .Annotation("Npgsql:Enum:check_status_enum", "pending,cashed,voided")
                .Annotation("Npgsql:Enum:check_type_enum", "day,deferred")
                .Annotation("Npgsql:Enum:module_enum", "sales,purchases,inventory,salary")
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

            migrationBuilder.AddColumn<ModuleEnum>(
                name: "Module",
                table: "Entries",
                type: "module_enum",
                nullable: false,
                defaultValue: ModuleEnum.Sales);

            migrationBuilder.AddForeignKey(
                name: "FK_Entries_Modules_ModuleId",
                table: "Entries",
                column: "ModuleId",
                principalTable: "Modules",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Entries_Modules_ModuleId",
                table: "Entries");

            migrationBuilder.DropColumn(
                name: "Module",
                table: "Entries");

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
                .OldAnnotation("Npgsql:Enum:module_enum", "sales,purchases,inventory,salary")
                .OldAnnotation("Npgsql:Enum:purchase_request_state_enum", "pending,approved,rejected,completed")
                .OldAnnotation("Npgsql:Enum:sales_order_state_enum", "pending,confirmed,cancelled");

            migrationBuilder.AddForeignKey(
                name: "Entries_ModuleId_fkey",
                table: "Entries",
                column: "ModuleId",
                principalTable: "Modules",
                principalColumn: "Id");
        }
    }
}
