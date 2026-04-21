using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBranch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "Stocks_BranchId_fkey",
                table: "Stocks");

            migrationBuilder.DropTable(
                name: "Branches");

            migrationBuilder.DropIndex(
                name: "IX_Stocks_BranchId",
                table: "Stocks");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Stocks");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "Stocks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Branches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Address = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Branches_pkey", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Stocks_BranchId",
                table: "Stocks",
                column: "BranchId");

            migrationBuilder.AddForeignKey(
                name: "Stocks_BranchId_fkey",
                table: "Stocks",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id");
        }
    }
}
