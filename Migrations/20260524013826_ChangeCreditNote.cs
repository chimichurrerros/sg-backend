using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class ChangeCreditNote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PaymentOrderCreditNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PaymentOrderId = table.Column<int>(type: "integer", nullable: false),
                    CreditNoteId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PaymentOrderCreditNotes_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "PaymentOrderCreditNotes_CreditNoteId_fkey",
                        column: x => x.CreditNoteId,
                        principalTable: "CreditNotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "PaymentOrderCreditNotes_PaymentOrderId_fkey",
                        column: x => x.PaymentOrderId,
                        principalTable: "PaymentOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOrderCreditNotes_CreditNoteId",
                table: "PaymentOrderCreditNotes",
                column: "CreditNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOrderCreditNotes_PaymentOrderId",
                table: "PaymentOrderCreditNotes",
                column: "PaymentOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentOrderCreditNotes");
        }
    }
}
