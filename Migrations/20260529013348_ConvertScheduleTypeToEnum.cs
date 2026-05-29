using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class ConvertScheduleTypeToEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "Schedules_ScheduleTypeId_fkey",
                table: "Schedules");

            migrationBuilder.DropTable(
                name: "ScheduleTypes");

            migrationBuilder.DropIndex(
                name: "IX_Schedules_ScheduleTypeId",
                table: "Schedules");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScheduleTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ScheduleTypes_pkey", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_ScheduleTypeId",
                table: "Schedules",
                column: "ScheduleTypeId");

            migrationBuilder.AddForeignKey(
                name: "Schedules_ScheduleTypeId_fkey",
                table: "Schedules",
                column: "ScheduleTypeId",
                principalTable: "ScheduleTypes",
                principalColumn: "Id");
        }
    }
}
