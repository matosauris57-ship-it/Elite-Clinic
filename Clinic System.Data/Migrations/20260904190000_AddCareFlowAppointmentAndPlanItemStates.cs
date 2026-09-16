using Clinic_System.Data.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinic_System.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260904190000_AddCareFlowAppointmentAndPlanItemStates")]
    public partial class AddCareFlowAppointmentAndPlanItemStates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TreatmentPlanId",
                table: "Appointments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PlanItemId",
                table: "Appointments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TreatmentProcedureId",
                table: "Appointments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ToothNumber",
                table: "Appointments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AcceptanceStatus",
                table: "PlanItems",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.AddColumn<string>(
                name: "ExecutionStatus",
                table: "PlanItems",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.AddColumn<int>(
                name: "DentalTreatmentId",
                table: "PlanItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ScheduledAppointmentId",
                table: "PlanItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InvoicedPaymentId",
                table: "PlanItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PlanItemId",
                table: "Appointments",
                column: "PlanItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_TreatmentPlanId",
                table: "Appointments",
                column: "TreatmentPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_TreatmentProcedureId",
                table: "Appointments",
                column: "TreatmentProcedureId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanItems_DentalTreatmentId",
                table: "PlanItems",
                column: "DentalTreatmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanItems_ScheduledAppointmentId",
                table: "PlanItems",
                column: "ScheduledAppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanItems_InvoicedPaymentId",
                table: "PlanItems",
                column: "InvoicedPaymentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_TreatmentPlans_TreatmentPlanId",
                table: "Appointments",
                column: "TreatmentPlanId",
                principalTable: "TreatmentPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_PlanItems_PlanItemId",
                table: "Appointments",
                column: "PlanItemId",
                principalTable: "PlanItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_TreatmentProcedures_TreatmentProcedureId",
                table: "Appointments",
                column: "TreatmentProcedureId",
                principalTable: "TreatmentProcedures",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PlanItems_DentalTreatments_DentalTreatmentId",
                table: "PlanItems",
                column: "DentalTreatmentId",
                principalTable: "DentalTreatments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PlanItems_Appointments_ScheduledAppointmentId",
                table: "PlanItems",
                column: "ScheduledAppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_PlanItems_Payments_InvoicedPaymentId",
                table: "PlanItems",
                column: "InvoicedPaymentId",
                principalTable: "Payments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Appointments_TreatmentPlans_TreatmentPlanId", table: "Appointments");
            migrationBuilder.DropForeignKey(name: "FK_Appointments_PlanItems_PlanItemId", table: "Appointments");
            migrationBuilder.DropForeignKey(name: "FK_Appointments_TreatmentProcedures_TreatmentProcedureId", table: "Appointments");
            migrationBuilder.DropForeignKey(name: "FK_PlanItems_DentalTreatments_DentalTreatmentId", table: "PlanItems");
            migrationBuilder.DropForeignKey(name: "FK_PlanItems_Appointments_ScheduledAppointmentId", table: "PlanItems");
            migrationBuilder.DropForeignKey(name: "FK_PlanItems_Payments_InvoicedPaymentId", table: "PlanItems");

            migrationBuilder.DropIndex(name: "IX_Appointments_PlanItemId", table: "Appointments");
            migrationBuilder.DropIndex(name: "IX_Appointments_TreatmentPlanId", table: "Appointments");
            migrationBuilder.DropIndex(name: "IX_Appointments_TreatmentProcedureId", table: "Appointments");
            migrationBuilder.DropIndex(name: "IX_PlanItems_DentalTreatmentId", table: "PlanItems");
            migrationBuilder.DropIndex(name: "IX_PlanItems_ScheduledAppointmentId", table: "PlanItems");
            migrationBuilder.DropIndex(name: "IX_PlanItems_InvoicedPaymentId", table: "PlanItems");

            migrationBuilder.DropColumn(name: "TreatmentPlanId", table: "Appointments");
            migrationBuilder.DropColumn(name: "PlanItemId", table: "Appointments");
            migrationBuilder.DropColumn(name: "TreatmentProcedureId", table: "Appointments");
            migrationBuilder.DropColumn(name: "ToothNumber", table: "Appointments");
            migrationBuilder.DropColumn(name: "AcceptanceStatus", table: "PlanItems");
            migrationBuilder.DropColumn(name: "ExecutionStatus", table: "PlanItems");
            migrationBuilder.DropColumn(name: "DentalTreatmentId", table: "PlanItems");
            migrationBuilder.DropColumn(name: "ScheduledAppointmentId", table: "PlanItems");
            migrationBuilder.DropColumn(name: "InvoicedPaymentId", table: "PlanItems");
        }
    }
}
