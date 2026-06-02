using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyAttendance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE table_name = 'Entries' AND constraint_name = 'FK_Entries_Modules_ModuleId') THEN
                        ALTER TABLE ""Entries"" DROP CONSTRAINT ""FK_Entries_Modules_ModuleId"";
                    END IF;
                    DROP TABLE IF EXISTS ""Attendances"";
                    DROP TABLE IF EXISTS ""EntryModelDetails"";
                    DROP TABLE IF EXISTS ""Modules"";
                    DROP TABLE IF EXISTS ""AttendanceTypes"";
                    DROP TABLE IF EXISTS ""EntryModels"";
                    DROP INDEX IF EXISTS ""IX_Entries_ModuleId"";
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Entries' AND column_name = 'ModuleId') THEN
                        ALTER TABLE ""Entries"" DROP COLUMN ""ModuleId"";
                    END IF;
                END
                $$;
            ");

            migrationBuilder.CreateTable(
                name: "DailyAttendances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("DailyAttendances_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "DailyAttendances_EmployeeId_fkey",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailyAttendances_EmployeeId_Date",
                table: "DailyAttendances",
                columns: new[] { "EmployeeId", "Date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyAttendances");

            migrationBuilder.AddColumn<int>(
                name: "ModuleId",
                table: "Entries",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AttendanceTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AffectsPayroll = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("AttendanceTypes_pkey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EntryModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("EntryModels_pkey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Modules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Modules_pkey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Attendances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AttendanceTypeId = table.Column<int>(type: "integer", nullable: false),
                    EmployeeId = table.Column<int>(type: "integer", nullable: false),
                    CheckIn = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    CheckOut = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    MinutesLate = table.Column<int>(type: "integer", nullable: true, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Attendances_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "Attendances_AttendanceTypeId_fkey",
                        column: x => x.AttendanceTypeId,
                        principalTable: "AttendanceTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "Attendances_EmployeeId_fkey",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EntryModelDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccountPlanId = table.Column<int>(type: "integer", nullable: false),
                    EntryModelId = table.Column<int>(type: "integer", nullable: false),
                    IsDebit = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("EntryModelDetails_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "EntryModelDetails_AccountPlanId_fkey",
                        column: x => x.AccountPlanId,
                        principalTable: "AccountPlans",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "EntryModelDetails_EntryModelId_fkey",
                        column: x => x.EntryModelId,
                        principalTable: "EntryModels",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Entries_ModuleId",
                table: "Entries",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_AttendanceTypeId",
                table: "Attendances",
                column: "AttendanceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_EmployeeId",
                table: "Attendances",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EntryModelDetails_AccountPlanId",
                table: "EntryModelDetails",
                column: "AccountPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_EntryModelDetails_EntryModelId",
                table: "EntryModelDetails",
                column: "EntryModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Entries_Modules_ModuleId",
                table: "Entries",
                column: "ModuleId",
                principalTable: "Modules",
                principalColumn: "Id");
        }
    }
}
