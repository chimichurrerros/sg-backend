using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class RemoveEmployeeRelationDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmployeeRelations_OneActiveSpouse",
                table: "EmployeeRelations");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "EmployeeRelations");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "EmployeeRelations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "EndDate",
                table: "EmployeeRelations",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "StartDate",
                table: "EmployeeRelations",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeRelations_OneActiveSpouse",
                table: "EmployeeRelations",
                column: "EmployeeId",
                unique: true,
                filter: "\"RelationType\" = 1 AND \"EndDate\" IS NULL");
        }
    }
}
