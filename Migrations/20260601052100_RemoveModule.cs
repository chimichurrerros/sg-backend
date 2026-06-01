using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class RemoveModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Entries_Modules_ModuleId",
                table: "Entries");

            migrationBuilder.DropTable(
                name: "EntryModelDetails");

            migrationBuilder.DropTable(
                name: "Modules");

            migrationBuilder.DropTable(
                name: "EntryModels");

            migrationBuilder.DropIndex(
                name: "IX_Entries_ModuleId",
                table: "Entries");

            migrationBuilder.DropColumn(
                name: "ModuleId",
                table: "Entries");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ModuleId",
                table: "Entries",
                type: "integer",
                nullable: true);

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
