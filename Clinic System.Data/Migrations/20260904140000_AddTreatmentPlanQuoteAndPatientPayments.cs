using Clinic_System.Data.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinic_System.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260904140000_AddTreatmentPlanQuoteAndPatientPayments")]
    public partial class AddTreatmentPlanQuoteAndPatientPayments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PatientId",
                table: "Payments",
                type: "int",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE p
                SET p.PatientId = a.PatientId
                FROM Payments p
                INNER JOIN Appointments a ON a.Id = p.AppointmentId
                """);

            migrationBuilder.Sql("DELETE FROM Payments WHERE PatientId IS NULL");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Appointments_AppointmentId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_AppointmentId",
                table: "Payments");

            migrationBuilder.AlterColumn<int>(
                name: "AppointmentId",
                table: "Payments",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "PatientId",
                table: "Payments",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_AppointmentId",
                table: "Payments",
                column: "AppointmentId",
                unique: true,
                filter: "[AppointmentId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PatientId",
                table: "Payments",
                column: "PatientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Appointments_AppointmentId",
                table: "Payments",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Patients_PatientId",
                table: "Payments",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddColumn<DateTime>(
                name: "AcceptedAt",
                table: "TreatmentPlans",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AcceptedByName",
                table: "TreatmentPlans",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InvoicePaymentId",
                table: "TreatmentPlans",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "IssuedAt",
                table: "TreatmentPlans",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "TreatmentPlans",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentPlans_InvoicePaymentId",
                table: "TreatmentPlans",
                column: "InvoicePaymentId");

            migrationBuilder.AddForeignKey(
                name: "FK_TreatmentPlans_Payments_InvoicePaymentId",
                table: "TreatmentPlans",
                column: "InvoicePaymentId",
                principalTable: "Payments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TreatmentPlans_Payments_InvoicePaymentId",
                table: "TreatmentPlans");

            migrationBuilder.DropIndex(
                name: "IX_TreatmentPlans_InvoicePaymentId",
                table: "TreatmentPlans");

            migrationBuilder.DropColumn(name: "AcceptedAt", table: "TreatmentPlans");
            migrationBuilder.DropColumn(name: "AcceptedByName", table: "TreatmentPlans");
            migrationBuilder.DropColumn(name: "InvoicePaymentId", table: "TreatmentPlans");
            migrationBuilder.DropColumn(name: "IssuedAt", table: "TreatmentPlans");
            migrationBuilder.DropColumn(name: "RejectionReason", table: "TreatmentPlans");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Patients_PatientId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Appointments_AppointmentId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_PatientId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_AppointmentId",
                table: "Payments");

            migrationBuilder.Sql("DELETE FROM Payments WHERE AppointmentId IS NULL");

            migrationBuilder.DropColumn(name: "PatientId", table: "Payments");

            migrationBuilder.AlterColumn<int>(
                name: "AppointmentId",
                table: "Payments",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_AppointmentId",
                table: "Payments",
                column: "AppointmentId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Appointments_AppointmentId",
                table: "Payments",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
