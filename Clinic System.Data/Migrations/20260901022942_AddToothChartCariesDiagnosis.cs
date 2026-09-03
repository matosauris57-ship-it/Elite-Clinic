using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinic_System.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddToothChartCariesDiagnosis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CariesType",
                table: "ToothChartEntries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClinicalDiagnosis",
                table: "ToothChartEntries",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Icdas",
                table: "ToothChartEntries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProposedTreatment",
                table: "ToothChartEntries",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CariesType",
                table: "ToothChartEntries");

            migrationBuilder.DropColumn(
                name: "ClinicalDiagnosis",
                table: "ToothChartEntries");

            migrationBuilder.DropColumn(
                name: "Icdas",
                table: "ToothChartEntries");

            migrationBuilder.DropColumn(
                name: "ProposedTreatment",
                table: "ToothChartEntries");
        }
    }
}
