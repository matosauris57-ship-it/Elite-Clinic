using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinic_System.Data.Migrations
{
    /// <inheritdoc />
    public partial class FinalizeDatabaseConstraintsAndSolvingRaceCondition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Prescriptions_EndDate_After_StartDate",
                table: "Prescriptions");

            migrationBuilder.DropIndex(
                name: "IX_Patients_ApplicationUserId",
                table: "Patients");

            migrationBuilder.DropIndex(
                name: "IX_Doctors_ApplicationUserId",
                table: "Doctors");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_Doctor_Date",
                table: "Appointments");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Prescriptions_EndDate_After_StartDate",
                table: "Prescriptions",
                sql: "[EndDate] >= [StartDate]");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_ApplicationUserId_Unique",
                table: "Patients",
                column: "ApplicationUserId",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Doctors_ApplicationUserId_Unique",
                table: "Doctors",
                column: "ApplicationUserId",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_Doctor_Date_Unique",
                table: "Appointments",
                columns: new[] { "DoctorId", "AppointmentDateTime" },
                unique: true,
                filter: "[AppointmentStatus] != 'Cancelled' AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_Patient_Date_Unique",
                table: "Appointments",
                columns: new[] { "PatientId", "AppointmentDateTime" },
                unique: true,
                filter: "[AppointmentStatus] != 'Cancelled' AND [IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Prescriptions_EndDate_After_StartDate",
                table: "Prescriptions");

            migrationBuilder.DropIndex(
                name: "IX_Patients_ApplicationUserId_Unique",
                table: "Patients");

            migrationBuilder.DropIndex(
                name: "IX_Doctors_ApplicationUserId_Unique",
                table: "Doctors");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_Doctor_Date_Unique",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_Patient_Date_Unique",
                table: "Appointments");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Prescriptions_EndDate_After_StartDate",
                table: "Prescriptions",
                sql: "[EndDate] > [StartDate]");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_ApplicationUserId",
                table: "Patients",
                column: "ApplicationUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Doctors_ApplicationUserId",
                table: "Doctors",
                column: "ApplicationUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_Doctor_Date",
                table: "Appointments",
                columns: new[] { "DoctorId", "AppointmentDateTime" });
        }
    }
}
