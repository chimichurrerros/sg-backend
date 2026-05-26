using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class RemoveEmployeeEntityDependency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "Employees_EntityId_fkey",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_EntityId",
                table: "Employees");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Employees",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "BirthDate",
                table: "Employees",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentNumber",
                table: "Employees",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Employees",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GenderId",
                table: "Employees",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Employees",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Lastname",
                table: "Employees",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Employees",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Employees",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "Employees" AS e
                SET
                    "Name" = pp."Name",
                    "Lastname" = pp."Lastname",
                    "BirthDate" = pp."BirthDate",
                    "GenderId" = pp."GenderId",
                    "DocumentNumber" = en."DocumentNumber",
                    "Phone" = en."Phone",
                    "Address" = en."Address",
                    "Email" = en."Email",
                    "IsActive" = en."IsActive"
                FROM "PhysicalPersons" AS pp
                INNER JOIN "Entities" AS en ON en."Id" = pp."EntityId"
                WHERE e."EntityId" = pp."EntityId";
            """);

            migrationBuilder.Sql("""
                UPDATE "Employees"
                SET
                    "Name" = COALESCE("Name", ''),
                    "Lastname" = COALESCE("Lastname", ''),
                    "DocumentNumber" = COALESCE("DocumentNumber", ''),
                    "BirthDate" = COALESCE("BirthDate", DATE '0001-01-01'),
                    "IsActive" = COALESCE("IsActive", true),
                    "GenderId" = COALESCE("GenderId", (SELECT "Id" FROM "Genders" ORDER BY "Id" LIMIT 1));
            """);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "BirthDate",
                table: "Employees",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DocumentNumber",
                table: "Employees",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "GenderId",
                table: "Employees",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Employees",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Lastname",
                table: "Employees",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Employees",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "EntityId",
                table: "Employees");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_GenderId",
                table: "Employees",
                column: "GenderId");

            migrationBuilder.AddForeignKey(
                name: "Employees_GenderId_fkey",
                table: "Employees",
                column: "GenderId",
                principalTable: "Genders",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "Employees_GenderId_fkey",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_GenderId",
                table: "Employees");

            migrationBuilder.AddColumn<int>(
                name: "EntityId",
                table: "Employees",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "Employees" AS e
                SET "EntityId" = pp."EntityId"
                FROM "PhysicalPersons" AS pp
                INNER JOIN "Entities" AS en ON en."Id" = pp."EntityId"
                WHERE en."DocumentNumber" = e."DocumentNumber";
            """);

            migrationBuilder.Sql("""
                UPDATE "Employees"
                SET "EntityId" = (
                    SELECT pp."EntityId"
                    FROM "PhysicalPersons" AS pp
                    ORDER BY pp."EntityId"
                    LIMIT 1
                )
                WHERE "EntityId" IS NULL;
            """);

            migrationBuilder.AlterColumn<int>(
                name: "EntityId",
                table: "Employees",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "BirthDate",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "DocumentNumber",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "Lastname",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "GenderId",
                table: "Employees");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_EntityId",
                table: "Employees",
                column: "EntityId");

            migrationBuilder.AddForeignKey(
                name: "Employees_EntityId_fkey",
                table: "Employees",
                column: "EntityId",
                principalTable: "PhysicalPersons",
                principalColumn: "EntityId");
        }
    }
}
