using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class AddEnumsToDbContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:check_status", "pending,cashed,voided")
                .Annotation("Npgsql:Enum:check_type", "day,deferred");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:Enum:check_status", "pending,cashed,voided")
                .OldAnnotation("Npgsql:Enum:check_type", "day,deferred");
        }
    }
}
