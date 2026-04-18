using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnnecessaryData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "Customers_TaxConditionId_fkey",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "Products_UnitOfMeasurementId_fkey",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "Stocks_LoteId_fkey",
                table: "Stocks");

            migrationBuilder.DropForeignKey(
                name: "Stocks_WarehouseId_fkey",
                table: "Stocks");

            migrationBuilder.DropTable(
                name: "TaxConditions");

            migrationBuilder.DropTable(
                name: "TransactionDetails");

            migrationBuilder.DropTable(
                name: "TransferDetails");

            migrationBuilder.DropTable(
                name: "UnitsOfMeasurements");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "Lotes");

            migrationBuilder.DropTable(
                name: "TransactionTypes");

            migrationBuilder.DropTable(
                name: "Transfers");

            migrationBuilder.DropTable(
                name: "Warehouses");

            migrationBuilder.DropIndex(
                name: "IX_Stocks_LoteId",
                table: "Stocks");

            migrationBuilder.DropIndex(
                name: "IX_Stocks_WarehouseId",
                table: "Stocks");

            migrationBuilder.DropIndex(
                name: "IX_Products_UnitOfMeasurementId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Customers_TaxConditionId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CreditLimit",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "TaxConditionId",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "Stocks");

            migrationBuilder.DropColumn(
                name: "UnitOfMeasurementId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "TaxConditionId",
                table: "Customers");

            migrationBuilder.AlterColumn<decimal>(
                name: "CreditLimit",
                table: "Customers",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(15,2)",
                oldPrecision: 15,
                oldScale: 2);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CreditLimit",
                table: "Suppliers",
                type: "numeric(15,2)",
                precision: 15,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "TaxConditionId",
                table: "Suppliers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WarehouseId",
                table: "Stocks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UnitOfMeasurementId",
                table: "Products",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<decimal>(
                name: "CreditLimit",
                table: "Customers",
                type: "numeric(15,2)",
                precision: 15,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AddColumn<int>(
                name: "TaxConditionId",
                table: "Customers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Lotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    LoteNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ReceiptDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Lotes_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "Lotes_ProductId_fkey",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TaxConditions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("TaxConditions_pkey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TransactionTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("TransactionTypes_pkey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UnitsOfMeasurements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("UnitsOfMeasurements_pkey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Warehouses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Warehouses_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "Warehouses_BranchId_fkey",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Transfers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DestinationWarehouseId = table.Column<int>(type: "integer", nullable: false),
                    SourceWarehouseId = table.Column<int>(type: "integer", nullable: false),
                    StateId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Observation = table.Column<string>(type: "text", nullable: true),
                    ReceiptDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ShipmentDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Transfers_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "Transfers_DestinationWarehouseId_fkey",
                        column: x => x.DestinationWarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "Transfers_SourceWarehouseId_fkey",
                        column: x => x.SourceWarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "Transfers_StateId_fkey",
                        column: x => x.StateId,
                        principalTable: "States",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "Transfers_UserId_fkey",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StateId = table.Column<int>(type: "integer", nullable: false),
                    TransactionTypeId = table.Column<int>(type: "integer", nullable: false),
                    TransferId = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    AddStock = table.Column<bool>(type: "boolean", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Observation = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Transactions_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "Transactions_StateId_fkey",
                        column: x => x.StateId,
                        principalTable: "States",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "Transactions_TransactionTypeId_fkey",
                        column: x => x.TransactionTypeId,
                        principalTable: "TransactionTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "Transactions_TransferId_fkey",
                        column: x => x.TransferId,
                        principalTable: "Transfers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "Transactions_UserId_fkey",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TransferDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LoteId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    TransferId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("TransferDetails_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "TransferDetails_LoteId_fkey",
                        column: x => x.LoteId,
                        principalTable: "Lotes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "TransferDetails_ProductId_fkey",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "TransferDetails_TransferId_fkey",
                        column: x => x.TransferId,
                        principalTable: "Transfers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TransactionDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LoteId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    TransactionId = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("TransactionDetails_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "TransactionDetails_LoteId_fkey",
                        column: x => x.LoteId,
                        principalTable: "Lotes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "TransactionDetails_ProductId_fkey",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "TransactionDetails_TransactionId_fkey",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Stocks_LoteId",
                table: "Stocks",
                column: "LoteId");

            migrationBuilder.CreateIndex(
                name: "IX_Stocks_WarehouseId",
                table: "Stocks",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_UnitOfMeasurementId",
                table: "Products",
                column: "UnitOfMeasurementId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_TaxConditionId",
                table: "Customers",
                column: "TaxConditionId");

            migrationBuilder.CreateIndex(
                name: "IX_Lotes_ProductId",
                table: "Lotes",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionDetails_LoteId",
                table: "TransactionDetails",
                column: "LoteId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionDetails_ProductId",
                table: "TransactionDetails",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionDetails_TransactionId",
                table: "TransactionDetails",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_StateId",
                table: "Transactions",
                column: "StateId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_TransactionTypeId",
                table: "Transactions",
                column: "TransactionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_TransferId",
                table: "Transactions",
                column: "TransferId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_UserId",
                table: "Transactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferDetails_LoteId",
                table: "TransferDetails",
                column: "LoteId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferDetails_ProductId",
                table: "TransferDetails",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferDetails_TransferId",
                table: "TransferDetails",
                column: "TransferId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_DestinationWarehouseId",
                table: "Transfers",
                column: "DestinationWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_SourceWarehouseId",
                table: "Transfers",
                column: "SourceWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_StateId",
                table: "Transfers",
                column: "StateId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_UserId",
                table: "Transfers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_BranchId",
                table: "Warehouses",
                column: "BranchId");

            migrationBuilder.AddForeignKey(
                name: "Customers_TaxConditionId_fkey",
                table: "Customers",
                column: "TaxConditionId",
                principalTable: "TaxConditions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "Products_UnitOfMeasurementId_fkey",
                table: "Products",
                column: "UnitOfMeasurementId",
                principalTable: "UnitsOfMeasurements",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "Stocks_LoteId_fkey",
                table: "Stocks",
                column: "LoteId",
                principalTable: "Lotes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "Stocks_WarehouseId_fkey",
                table: "Stocks",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id");
        }
    }
}
