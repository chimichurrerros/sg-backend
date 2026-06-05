using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class EditAccountPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Level",
                table: "AccountPlans",
                newName: "Order");

            migrationBuilder.AddColumn<int>(
                name: "AccountantProcessId",
                table: "AccountPlans",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ParentId",
                table: "AccountPlans",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountPlans_AccountantProcessId",
                table: "AccountPlans",
                column: "AccountantProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountPlans_ParentId",
                table: "AccountPlans",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_AccountPlans_AccountPlans_ParentId",
                table: "AccountPlans",
                column: "ParentId",
                principalTable: "AccountPlans",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AccountPlans_AccountantProcesses_AccountantProcessId",
                table: "AccountPlans",
                column: "AccountantProcessId",
                principalTable: "AccountantProcesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccountPlans_AccountPlans_ParentId",
                table: "AccountPlans");

            migrationBuilder.DropForeignKey(
                name: "FK_AccountPlans_AccountantProcesses_AccountantProcessId",
                table: "AccountPlans");

            migrationBuilder.DropIndex(
                name: "IX_AccountPlans_AccountantProcessId",
                table: "AccountPlans");

            migrationBuilder.DropIndex(
                name: "IX_AccountPlans_ParentId",
                table: "AccountPlans");

            migrationBuilder.DropColumn(
                name: "AccountantProcessId",
                table: "AccountPlans");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "AccountPlans");

            migrationBuilder.RenameColumn(
                name: "Order",
                table: "AccountPlans",
                newName: "Level");
        }
    }
}
