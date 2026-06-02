using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class RenumberPayrollStatuses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Renumber existing PayrollStatusId values to match new enum:
            //   Old: Open=1, Processed=2, Closed=3, Paid=4
            //   New: Open=1, Closed=2,  Paid=3
            // Processed (2) → Open (1) to give legacy records a path forward
            // Closed    (3) → Closed (2)
            // Paid      (4) → Paid   (3)
            migrationBuilder.Sql("""
                UPDATE "PayrollProcesses"
                SET "PayrollStatusId" = CASE
                    WHEN "PayrollStatusId" = 2 THEN 1
                    WHEN "PayrollStatusId" = 3 THEN 2
                    WHEN "PayrollStatusId" = 4 THEN 3
                    ELSE "PayrollStatusId"
                END
            """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverse: restore old numbering
            // Open (1) → stays Open (1) - records that were originally Processed (2) will be incorrectly mapped
            // Closed (2) → Closed (3)
            // Paid   (3) → Paid   (4)
            migrationBuilder.Sql("""
                UPDATE "PayrollProcesses"
                SET "PayrollStatusId" = CASE
                    WHEN "PayrollStatusId" = 2 THEN 3
                    WHEN "PayrollStatusId" = 3 THEN 4
                    ELSE "PayrollStatusId"
                END
            """);
        }
    }
}
