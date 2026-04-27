using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class ColumsnIntegerToEnums : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Checks",
                table: "Checks");

            migrationBuilder.Sql(@"
ALTER TABLE ""Checks""
ALTER COLUMN ""Type"" TYPE check_type_enum
USING (
    CASE ""Type""
        WHEN 0 THEN 'day'::check_type_enum
        WHEN 1 THEN 'deferred'::check_type_enum
    END
);");

            migrationBuilder.Sql(@"
ALTER TABLE ""Checks""
ALTER COLUMN ""Status"" TYPE check_status_enum
USING (
    CASE ""Status""
        WHEN 0 THEN 'pending'::check_status_enum
        WHEN 1 THEN 'cashed'::check_status_enum
        WHEN 2 THEN 'voided'::check_status_enum
    END
);");

            migrationBuilder.Sql(@"
ALTER TABLE ""BankMovements""
ALTER COLUMN ""MovementType"" TYPE bank_movement_type_enum
USING (
    CASE ""MovementType""
        WHEN 1 THEN 'debit'::bank_movement_type_enum
        WHEN 2 THEN 'credit'::bank_movement_type_enum
    END
);");

            migrationBuilder.AddPrimaryKey(
                name: "Checks_pkey",
                table: "Checks",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "Checks_pkey",
                table: "Checks");

            migrationBuilder.Sql(@"
ALTER TABLE ""Checks""
ALTER COLUMN ""Type"" TYPE integer
USING (
    CASE ""Type""
        WHEN 'day'::check_type_enum THEN 0
        WHEN 'deferred'::check_type_enum THEN 1
    END
);");

            migrationBuilder.Sql(@"
ALTER TABLE ""Checks""
ALTER COLUMN ""Status"" TYPE integer
USING (
    CASE ""Status""
        WHEN 'pending'::check_status_enum THEN 0
        WHEN 'cashed'::check_status_enum THEN 1
        WHEN 'voided'::check_status_enum THEN 2
    END
);");

            migrationBuilder.Sql(@"
ALTER TABLE ""BankMovements""
ALTER COLUMN ""MovementType"" TYPE integer
USING (
    CASE ""MovementType""
        WHEN 'debit'::bank_movement_type_enum THEN 1
        WHEN 'credit'::bank_movement_type_enum THEN 2
    END
);");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Checks",
                table: "Checks",
                column: "Id");
        }
    }
}
