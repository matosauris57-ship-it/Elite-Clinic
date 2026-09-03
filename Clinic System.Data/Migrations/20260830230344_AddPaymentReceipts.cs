using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinic_System.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentReceipts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PaymentReceipts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentReceipts", x => x.Id);
                    table.CheckConstraint("CK_PaymentReceipts_Amount", "[Amount] > 0");
                    table.ForeignKey(
                        name: "FK_PaymentReceipts_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentReceipts_PaidAt",
                table: "PaymentReceipts",
                column: "PaidAt");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentReceipts_PaymentId",
                table: "PaymentReceipts",
                column: "PaymentId");

            migrationBuilder.Sql("""
                INSERT INTO [PaymentReceipts] ([PaymentId], [Amount], [PaymentMethod], [Notes], [PaidAt], [IsDeleted], [CreatedAt])
                SELECT
                    p.[Id],
                    p.[AmountPaid],
                    ISNULL(p.[PaymentMethod], N'Cash'),
                    p.[AdditionalNotes],
                    ISNULL(p.[PaymentDate], p.[CreatedAt]),
                    0,
                    ISNULL(p.[PaymentDate], p.[CreatedAt])
                FROM [Payments] p
                WHERE p.[IsDeleted] = 0
                  AND p.[PaymentStatus] IN (N'Paid', N'Refunded')
                  AND p.[AmountPaid] > 0;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentReceipts");
        }
    }
}
