using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseOrderForSupplierEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "Bills_PurchaseOrderId_fkey",
                table: "Bills");

            migrationBuilder.DropForeignKey(
                name: "PurchaseOrderDetails_PurchaseOrderId_fkey",
                table: "PurchaseOrderDetails");

            migrationBuilder.DropForeignKey(
                name: "PurchaseOrders_SupplierId_fkey",
                table: "PurchaseOrders");

            migrationBuilder.DropForeignKey(
                name: "PurchaseOrders_SupplierQuoteId_fkey",
                table: "PurchaseOrders");

            migrationBuilder.DropForeignKey(
                name: "PurchaseReturns_PurchaseOrderId_fkey",
                table: "PurchaseReturns");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrders_SupplierId",
                table: "PurchaseOrders");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrders_SupplierQuoteId",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "SupplierQuoteId",
                table: "PurchaseOrders");

            migrationBuilder.RenameColumn(
                name: "PurchaseOrderId",
                table: "PurchaseReturns",
                newName: "PurchaseOrderForSupplierId");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseReturns_PurchaseOrderId",
                table: "PurchaseReturns",
                newName: "IX_PurchaseReturns_PurchaseOrderForSupplierId");

            migrationBuilder.RenameColumn(
                name: "PurchaseOrderId",
                table: "PurchaseOrderDetails",
                newName: "PurchaseOrderForSupplierId");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseOrderDetails_PurchaseOrderId",
                table: "PurchaseOrderDetails",
                newName: "IX_PurchaseOrderDetails_PurchaseOrderForSupplierId");

            migrationBuilder.RenameColumn(
                name: "PurchaseOrderId",
                table: "Bills",
                newName: "PurchaseOrderForSupplierId");

            migrationBuilder.RenameIndex(
                name: "IX_Bills_PurchaseOrderId",
                table: "Bills",
                newName: "IX_Bills_PurchaseOrderForSupplierId");

            migrationBuilder.CreateTable(
                name: "PurchaseOrdersForSupplier",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PurchaseOrderId = table.Column<int>(type: "integer", nullable: false),
                    SupplierId = table.Column<int>(type: "integer", nullable: false),
                    SupplierQuoteId = table.Column<int>(type: "integer", nullable: true),
                    Number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Total = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PurchaseOrdersForSupplier_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "PurchaseOrdersForSupplier_PurchaseOrderId_fkey",
                        column: x => x.PurchaseOrderId,
                        principalTable: "PurchaseOrders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "PurchaseOrdersForSupplier_SupplierId_fkey",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "PurchaseOrdersForSupplier_SupplierQuoteId_fkey",
                        column: x => x.SupplierQuoteId,
                        principalTable: "SupplierQuotes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrdersForSupplier_PurchaseOrderId",
                table: "PurchaseOrdersForSupplier",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrdersForSupplier_SupplierId",
                table: "PurchaseOrdersForSupplier",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrdersForSupplier_SupplierQuoteId",
                table: "PurchaseOrdersForSupplier",
                column: "SupplierQuoteId");

            migrationBuilder.AddForeignKey(
                name: "Bills_PurchaseOrderForSupplierId_fkey",
                table: "Bills",
                column: "PurchaseOrderForSupplierId",
                principalTable: "PurchaseOrdersForSupplier",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "PurchaseOrderDetails_PurchaseOrderForSupplierId_fkey",
                table: "PurchaseOrderDetails",
                column: "PurchaseOrderForSupplierId",
                principalTable: "PurchaseOrdersForSupplier",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "PurchaseReturns_PurchaseOrderForSupplierId_fkey",
                table: "PurchaseReturns",
                column: "PurchaseOrderForSupplierId",
                principalTable: "PurchaseOrdersForSupplier",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "Bills_PurchaseOrderForSupplierId_fkey",
                table: "Bills");

            migrationBuilder.DropForeignKey(
                name: "PurchaseOrderDetails_PurchaseOrderForSupplierId_fkey",
                table: "PurchaseOrderDetails");

            migrationBuilder.DropForeignKey(
                name: "PurchaseReturns_PurchaseOrderForSupplierId_fkey",
                table: "PurchaseReturns");

            migrationBuilder.DropTable(
                name: "PurchaseOrdersForSupplier");

            migrationBuilder.RenameColumn(
                name: "PurchaseOrderForSupplierId",
                table: "PurchaseReturns",
                newName: "PurchaseOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseReturns_PurchaseOrderForSupplierId",
                table: "PurchaseReturns",
                newName: "IX_PurchaseReturns_PurchaseOrderId");

            migrationBuilder.RenameColumn(
                name: "PurchaseOrderForSupplierId",
                table: "PurchaseOrderDetails",
                newName: "PurchaseOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseOrderDetails_PurchaseOrderForSupplierId",
                table: "PurchaseOrderDetails",
                newName: "IX_PurchaseOrderDetails_PurchaseOrderId");

            migrationBuilder.RenameColumn(
                name: "PurchaseOrderForSupplierId",
                table: "Bills",
                newName: "PurchaseOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_Bills_PurchaseOrderForSupplierId",
                table: "Bills",
                newName: "IX_Bills_PurchaseOrderId");

            migrationBuilder.AddColumn<int>(
                name: "SupplierId",
                table: "PurchaseOrders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SupplierQuoteId",
                table: "PurchaseOrders",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_SupplierId",
                table: "PurchaseOrders",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_SupplierQuoteId",
                table: "PurchaseOrders",
                column: "SupplierQuoteId");

            migrationBuilder.AddForeignKey(
                name: "Bills_PurchaseOrderId_fkey",
                table: "Bills",
                column: "PurchaseOrderId",
                principalTable: "PurchaseOrders",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "PurchaseOrderDetails_PurchaseOrderId_fkey",
                table: "PurchaseOrderDetails",
                column: "PurchaseOrderId",
                principalTable: "PurchaseOrders",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "PurchaseOrders_SupplierId_fkey",
                table: "PurchaseOrders",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "PurchaseOrders_SupplierQuoteId_fkey",
                table: "PurchaseOrders",
                column: "SupplierQuoteId",
                principalTable: "SupplierQuotes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "PurchaseReturns_PurchaseOrderId_fkey",
                table: "PurchaseReturns",
                column: "PurchaseOrderId",
                principalTable: "PurchaseOrders",
                principalColumn: "Id");
        }
    }
}
