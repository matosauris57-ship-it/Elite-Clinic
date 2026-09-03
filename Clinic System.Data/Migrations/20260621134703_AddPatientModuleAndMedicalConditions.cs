using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinic_System.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientModuleAndMedicalConditions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Condition",
                table: "ToothRecords",
                newName: "DiagnosisCondition");

            migrationBuilder.AddColumn<int>(
                name: "TreatmentCondition",
                table: "ToothRecords",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MobilePhone",
                table: "Patients",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NationalId",
                table: "Patients",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BloodPressure",
                table: "DentalHistories",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClinicalObservations",
                table: "DentalHistories",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Diagnosis",
                table: "DentalHistories",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtherDiseases",
                table: "DentalHistories",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReasonForConsultation",
                table: "DentalHistories",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MedicalConditions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalConditions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PatientMedicalConditions",
                columns: table => new
                {
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    MedicalConditionId = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientMedicalConditions", x => new { x.PatientId, x.MedicalConditionId });
                    table.ForeignKey(
                        name: "FK_PatientMedicalConditions_MedicalConditions_MedicalConditionId",
                        column: x => x.MedicalConditionId,
                        principalTable: "MedicalConditions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatientMedicalConditions_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Patients_NationalId_Unique",
                table: "Patients",
                column: "NationalId",
                unique: true,
                filter: "[IsDeleted] = 0 AND [NationalId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalConditions_Name",
                table: "MedicalConditions",
                column: "Name",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PatientMedicalConditions_MedicalConditionId",
                table: "PatientMedicalConditions",
                column: "MedicalConditionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PatientMedicalConditions");

            migrationBuilder.DropTable(
                name: "MedicalConditions");

            migrationBuilder.DropIndex(
                name: "IX_Patients_NationalId_Unique",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "TreatmentCondition",
                table: "ToothRecords");

            migrationBuilder.DropColumn(
                name: "MobilePhone",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "NationalId",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "BloodPressure",
                table: "DentalHistories");

            migrationBuilder.DropColumn(
                name: "ClinicalObservations",
                table: "DentalHistories");

            migrationBuilder.DropColumn(
                name: "Diagnosis",
                table: "DentalHistories");

            migrationBuilder.DropColumn(
                name: "OtherDiseases",
                table: "DentalHistories");

            migrationBuilder.DropColumn(
                name: "ReasonForConsultation",
                table: "DentalHistories");

            migrationBuilder.RenameColumn(
                name: "DiagnosisCondition",
                table: "ToothRecords",
                newName: "Condition");
        }
    }
}
