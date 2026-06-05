using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestForQuotation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RequestForQuotations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PurchaseRequestId = table.Column<int>(type: "integer", nullable: false),
                    SupplierId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Observation = table.Column<string>(type: "text", nullable: true),
                    State = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("RequestForQuotations_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "RequestForQuotations_PurchaseRequestId_fkey",
                        column: x => x.PurchaseRequestId,
                        principalTable: "PurchaseRequests",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "RequestForQuotations_SupplierId_fkey",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RequestForQuotationDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RequestForQuotationId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    QuantityRequested = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("RequestForQuotationDetails_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "RequestForQuotationDetails_ProductId_fkey",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "RequestForQuotationDetails_RequestForQuotationId_fkey",
                        column: x => x.RequestForQuotationId,
                        principalTable: "RequestForQuotations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_RequestForQuotationDetails_ProductId",
                table: "RequestForQuotationDetails",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestForQuotationDetails_RequestForQuotationId",
                table: "RequestForQuotationDetails",
                column: "RequestForQuotationId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestForQuotations_PurchaseRequestId",
                table: "RequestForQuotations",
                column: "PurchaseRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestForQuotations_SupplierId",
                table: "RequestForQuotations",
                column: "SupplierId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RequestForQuotationDetails");

            migrationBuilder.DropTable(
                name: "RequestForQuotations");
        }
    }
}
