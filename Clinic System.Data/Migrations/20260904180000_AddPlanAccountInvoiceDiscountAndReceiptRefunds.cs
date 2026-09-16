using Clinic_System.Data.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinic_System.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260904180000_AddPlanAccountInvoiceDiscountAndReceiptRefunds")]
    public partial class AddPlanAccountInvoiceDiscountAndReceiptRefunds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "Payments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "TreatmentPlanId",
                table: "Payments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Kind",
                table: "PaymentReceipts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Payment");

            migrationBuilder.AddColumn<bool>(
                name: "IsVoided",
                table: "PaymentReceipts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "VoidedAt",
                table: "PaymentReceipts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VoidReason",
                table: "PaymentReceipts",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_TreatmentPlanId",
                table: "Payments",
                column: "TreatmentPlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_TreatmentPlans_TreatmentPlanId",
                table: "Payments",
                column: "TreatmentPlanId",
                principalTable: "TreatmentPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.Sql("""
                UPDATE p
                SET p.TreatmentPlanId = t.Id
                FROM Payments p
                INNER JOIN TreatmentPlans t ON t.InvoicePaymentId = p.Id
                WHERE p.TreatmentPlanId IS NULL
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_TreatmentPlans_TreatmentPlanId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_TreatmentPlanId",
                table: "Payments");

            migrationBuilder.DropColumn(name: "DiscountAmount", table: "Payments");
            migrationBuilder.DropColumn(name: "TreatmentPlanId", table: "Payments");
            migrationBuilder.DropColumn(name: "Kind", table: "PaymentReceipts");
            migrationBuilder.DropColumn(name: "IsVoided", table: "PaymentReceipts");
            migrationBuilder.DropColumn(name: "VoidedAt", table: "PaymentReceipts");
            migrationBuilder.DropColumn(name: "VoidReason", table: "PaymentReceipts");
        }
    }
}
