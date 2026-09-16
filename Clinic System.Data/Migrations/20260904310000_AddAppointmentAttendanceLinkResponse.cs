using Clinic_System.Data.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinic_System.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260904310000_AddAppointmentAttendanceLinkResponse")]
    public partial class AddAppointmentAttendanceLinkResponse : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AttendanceLinkRespondedAt",
                table: "Appointments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AttendanceLinkAccepted",
                table: "Appointments",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttendanceLinkComment",
                table: "Appointments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_AttendanceLinkRespondedAt",
                table: "Appointments",
                column: "AttendanceLinkRespondedAt");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Appointments_AttendanceLinkRespondedAt",
                table: "Appointments");

            migrationBuilder.DropColumn(name: "AttendanceLinkRespondedAt", table: "Appointments");
            migrationBuilder.DropColumn(name: "AttendanceLinkAccepted", table: "Appointments");
            migrationBuilder.DropColumn(name: "AttendanceLinkComment", table: "Appointments");
        }
    }
}
