using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class ConvertPayrollCatalogsToEnums2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FkDepartmentsBoss",
                table: "Departments");

            migrationBuilder.DropForeignKey(
                name: "PayrollProcesses_ProcessTypeId_fkey",
                table: "PayrollProcesses");

            migrationBuilder.DropForeignKey(
                name: "PayrollUpdates_FormulaTypeId_fkey",
                table: "PayrollUpdates");

            migrationBuilder.DropForeignKey(
                name: "PayrollUpdates_PayrollTypeId_fkey",
                table: "PayrollUpdates");

            migrationBuilder.DropTable(
                name: "FormulaTypes");

            migrationBuilder.DropTable(
                name: "PayrollTypes");

            migrationBuilder.DropTable(
                name: "ProcessTypes");

            migrationBuilder.DropIndex(
                name: "IX_PayrollUpdates_FormulaTypeId",
                table: "PayrollUpdates");

            migrationBuilder.DropIndex(
                name: "IX_PayrollUpdates_PayrollTypeId",
                table: "PayrollUpdates");

            migrationBuilder.DropIndex(
                name: "IX_PayrollProcesses_ProcessTypeId",
                table: "PayrollProcesses");

            migrationBuilder.RenameColumn(
                name: "BossId",
                table: "Departments",
                newName: "EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_Departments_BossId",
                table: "Departments",
                newName: "IX_Departments_EmployeeId");

            migrationBuilder.CreateTable(
                name: "BranchDepartments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    DepartmentId = table.Column<int>(type: "integer", nullable: false),
                    BossId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("BranchDepartments_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "BranchDepartments_BossId_fkey",
                        column: x => x.BossId,
                        principalTable: "Employees",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "BranchDepartments_BranchId_fkey",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "BranchDepartments_DepartmentId_fkey",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BranchDepartments_BossId",
                table: "BranchDepartments",
                column: "BossId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchDepartments_BranchId_DepartmentId",
                table: "BranchDepartments",
                columns: new[] { "BranchId", "DepartmentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BranchDepartments_DepartmentId",
                table: "BranchDepartments",
                column: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Employees_EmployeeId",
                table: "Departments",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Employees_EmployeeId",
                table: "Departments");

            migrationBuilder.DropTable(
                name: "BranchDepartments");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "Departments",
                newName: "BossId");

            migrationBuilder.RenameIndex(
                name: "IX_Departments_EmployeeId",
                table: "Departments",
                newName: "IX_Departments_BossId");

            migrationBuilder.CreateTable(
                name: "FormulaTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("FormulaTypes_pkey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PayrollTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PayrollTypes_pkey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProcessTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ProcessTypes_pkey", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PayrollUpdates_FormulaTypeId",
                table: "PayrollUpdates",
                column: "FormulaTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollUpdates_PayrollTypeId",
                table: "PayrollUpdates",
                column: "PayrollTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollProcesses_ProcessTypeId",
                table: "PayrollProcesses",
                column: "ProcessTypeId");

            migrationBuilder.AddForeignKey(
                name: "FkDepartmentsBoss",
                table: "Departments",
                column: "BossId",
                principalTable: "Employees",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "PayrollProcesses_ProcessTypeId_fkey",
                table: "PayrollProcesses",
                column: "ProcessTypeId",
                principalTable: "ProcessTypes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "PayrollUpdates_FormulaTypeId_fkey",
                table: "PayrollUpdates",
                column: "FormulaTypeId",
                principalTable: "FormulaTypes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "PayrollUpdates_PayrollTypeId_fkey",
                table: "PayrollUpdates",
                column: "PayrollTypeId",
                principalTable: "PayrollTypes",
                principalColumn: "Id");
        }
    }
}
