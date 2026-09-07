using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinic_System.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientMedicalCertificates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PatientMedicalCertificates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    DoctorId = table.Column<int>(type: "int", nullable: true),
                    IssuedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CertificateType = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Diagnosis = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Recommendation = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    IncludesRest = table.Column<bool>(type: "bit", nullable: false),
                    RestStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RestEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RestDays = table.Column<int>(type: "int", nullable: true),
                    Observations = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Purpose = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    RecordedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientMedicalCertificates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PatientMedicalCertificates_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PatientMedicalCertificates_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PatientMedicalCertificates_DoctorId",
                table: "PatientMedicalCertificates",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientMedicalCertificates_PatientId_IssuedAt",
                table: "PatientMedicalCertificates",
                columns: new[] { "PatientId", "IssuedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PatientMedicalCertificates");
        }
    }
}
