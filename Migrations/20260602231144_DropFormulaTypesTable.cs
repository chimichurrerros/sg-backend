using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class DropFormulaTypesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop FK constraint from PayrollUpdates to FormulaTypes (if it still exists)
            migrationBuilder.Sql("""
                ALTER TABLE "PayrollUpdates" DROP CONSTRAINT IF EXISTS "PayrollUpdates_FormulaTypeId_fkey"
            """);

            // Drop the FormulaTypes table (no longer needed, FormulaTypeEnum is used instead)
            migrationBuilder.Sql("""
                DROP TABLE IF EXISTS "FormulaTypes"
            """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Recreate the FormulaTypes table
            migrationBuilder.Sql("""
                CREATE TABLE "FormulaTypes" (
                    "Id" INTEGER NOT NULL,
                    "Name" TEXT NOT NULL,
                    CONSTRAINT "FormulaTypes_pkey" PRIMARY KEY ("Id")
                )
            """);

            // Recreate the FK constraint (non-restrictive since it's for reference only)
            migrationBuilder.Sql("""
                ALTER TABLE "PayrollUpdates" ADD CONSTRAINT "PayrollUpdates_FormulaTypeId_fkey"
                    FOREIGN KEY ("FormulaTypeId") REFERENCES "FormulaTypes" ("Id")
            """);
        }
    }
}
