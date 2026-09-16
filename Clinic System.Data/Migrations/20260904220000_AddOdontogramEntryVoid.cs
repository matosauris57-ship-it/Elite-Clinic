using Clinic_System.Data.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinic_System.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260904220000_AddOdontogramEntryVoid")]
    public partial class AddOdontogramEntryVoid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsVoided",
                table: "ToothChartEntries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "VoidedAt",
                table: "ToothChartEntries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VoidedByUserId",
                table: "ToothChartEntries",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VoidReason",
                table: "ToothChartEntries",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsVoided",
                table: "DentalClinicalEvents",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "VoidedAt",
                table: "DentalClinicalEvents",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VoidedByUserId",
                table: "DentalClinicalEvents",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "IsVoided", table: "ToothChartEntries");
            migrationBuilder.DropColumn(name: "VoidedAt", table: "ToothChartEntries");
            migrationBuilder.DropColumn(name: "VoidedByUserId", table: "ToothChartEntries");
            migrationBuilder.DropColumn(name: "VoidReason", table: "ToothChartEntries");
            migrationBuilder.DropColumn(name: "IsVoided", table: "DentalClinicalEvents");
            migrationBuilder.DropColumn(name: "VoidedAt", table: "DentalClinicalEvents");
            migrationBuilder.DropColumn(name: "VoidedByUserId", table: "DentalClinicalEvents");
        }
    }
}
