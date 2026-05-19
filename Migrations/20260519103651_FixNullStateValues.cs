using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class FixNullStateValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Fix NULL State values in PurchaseOrders (set to Pending = 1)
            migrationBuilder.Sql("UPDATE \"PurchaseOrders\" SET \"State\" = 1 WHERE \"State\" IS NULL;");
            // Fix NULL State values in SupplierQuotes (set to Pending = 1)
            migrationBuilder.Sql("UPDATE \"SupplierQuotes\" SET \"State\" = 1 WHERE \"State\" IS NULL;");

            // Make columns NOT NULL with default value
            migrationBuilder.Sql("ALTER TABLE \"PurchaseOrders\" ALTER COLUMN \"State\" SET NOT NULL;");
            migrationBuilder.Sql("ALTER TABLE \"PurchaseOrders\" ALTER COLUMN \"State\" SET DEFAULT 1;");
            migrationBuilder.Sql("ALTER TABLE \"SupplierQuotes\" ALTER COLUMN \"State\" SET NOT NULL;");
            migrationBuilder.Sql("ALTER TABLE \"SupplierQuotes\" ALTER COLUMN \"State\" SET DEFAULT 1;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
