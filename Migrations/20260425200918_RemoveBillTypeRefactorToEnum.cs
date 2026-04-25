using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBillTypeRefactorToEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "Bills_BillTypeId_fkey",
                table: "Bills");

            migrationBuilder.DropTable(
                name: "BillTypes");

            migrationBuilder.DropIndex(
                name: "IX_Bills_BillTypeId",
                table: "Bills");

            migrationBuilder.RenameColumn(
                name: "BillTypeId",
                table: "Bills",
                newName: "BillType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BillType",
                table: "Bills",
                newName: "BillTypeId");

            migrationBuilder.CreateTable(
                name: "BillTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("BillTypes_pkey", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bills_BillTypeId",
                table: "Bills",
                column: "BillTypeId");

            migrationBuilder.AddForeignKey(
                name: "Bills_BillTypeId_fkey",
                table: "Bills",
                column: "BillTypeId",
                principalTable: "BillTypes",
                principalColumn: "Id");
        }
    }
}
