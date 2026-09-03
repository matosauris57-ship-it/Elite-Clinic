using Clinic_System.Data.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinic_System.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260902240000_AddAutomaticPatientNotifications")]
    public partial class AddAutomaticPatientNotifications : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DayBeforeReminderSentAt",
                table: "Appointments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SameDayReminderSentAt",
                table: "Appointments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BirthdayEmailLastSentYear",
                table: "Patients",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "DayBeforeReminderSentAt", table: "Appointments");
            migrationBuilder.DropColumn(name: "SameDayReminderSentAt", table: "Appointments");
            migrationBuilder.DropColumn(name: "BirthdayEmailLastSentYear", table: "Patients");
        }
    }
}
