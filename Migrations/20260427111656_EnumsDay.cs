using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class EnumsDay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"BankMovements\" DROP CONSTRAINT IF EXISTS \"BankMovements_MovementTypeId_fkey\";");

            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_BankMovements_MovementTypeId\";");

            migrationBuilder.Sql("DROP TABLE IF EXISTS \"MovementTypes\";");

            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'public'
          AND table_name = 'BankMovements'
          AND column_name = 'MovementTypeId'
    ) THEN
        ALTER TABLE ""BankMovements"" RENAME COLUMN ""MovementTypeId"" TO ""MovementType"";
    END IF;
END $$;
");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:bank_movement_type_enum", "debit,credit")
                .Annotation("Npgsql:Enum:bill_type_enum", "contado,credito")
                .Annotation("Npgsql:Enum:check_status_enum", "pending,cashed,voided")
                .Annotation("Npgsql:Enum:check_type_enum", "day,deferred")
                .OldAnnotation("Npgsql:Enum:check_status", "pending,cashed,voided")
                .OldAnnotation("Npgsql:Enum:check_type", "day,deferred");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Users",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "SalesOrders",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EmisionDate",
                table: "Checks",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MovementType",
                table: "BankMovements",
                newName: "MovementTypeId");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:check_status", "pending,cashed,voided")
                .Annotation("Npgsql:Enum:check_type", "day,deferred")
                .OldAnnotation("Npgsql:Enum:bank_movement_type_enum", "debit,credit")
                .OldAnnotation("Npgsql:Enum:bill_type_enum", "contado,credito")
                .OldAnnotation("Npgsql:Enum:check_status_enum", "pending,cashed,voided")
                .OldAnnotation("Npgsql:Enum:check_type_enum", "day,deferred");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "SalesOrders",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EmisionDate",
                table: "Checks",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.CreateTable(
                name: "MovementTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("MovementTypes_pkey", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BankMovements_MovementTypeId",
                table: "BankMovements",
                column: "MovementTypeId");

            migrationBuilder.AddForeignKey(
                name: "BankMovements_MovementTypeId_fkey",
                table: "BankMovements",
                column: "MovementTypeId",
                principalTable: "MovementTypes",
                principalColumn: "Id");
        }
    }
}
