using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinic_System.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPeriodontalExams : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PeriodontalExams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    DoctorId = table.Column<int>(type: "int", nullable: true),
                    ExaminedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RecordedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeriodontalExams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PeriodontalExams_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PeriodontalExams_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PeriodontalTeeth",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PeriodontalExamId = table.Column<int>(type: "int", nullable: false),
                    ToothNumber = table.Column<int>(type: "int", nullable: false),
                    Mobility = table.Column<int>(type: "int", nullable: false),
                    Furcation = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeriodontalTeeth", x => x.Id);
                    table.CheckConstraint("CK_PeriodontalTeeth_FDI", "(([ToothNumber] BETWEEN 11 AND 18) OR ([ToothNumber] BETWEEN 21 AND 28) OR ([ToothNumber] BETWEEN 31 AND 38) OR ([ToothNumber] BETWEEN 41 AND 48))");
                    table.ForeignKey(
                        name: "FK_PeriodontalTeeth_PeriodontalExams_PeriodontalExamId",
                        column: x => x.PeriodontalExamId,
                        principalTable: "PeriodontalExams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PeriodontalSites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PeriodontalToothId = table.Column<int>(type: "int", nullable: false),
                    Surface = table.Column<int>(type: "int", nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false),
                    ProbingDepthMm = table.Column<int>(type: "int", nullable: true),
                    RecessionMm = table.Column<int>(type: "int", nullable: true),
                    ClinicalAttachmentLevelMm = table.Column<int>(type: "int", nullable: true),
                    Bleeding = table.Column<bool>(type: "bit", nullable: false),
                    Plaque = table.Column<bool>(type: "bit", nullable: false),
                    Suppuration = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeriodontalSites", x => x.Id);
                    table.CheckConstraint("CK_PeriodontalSites_Cal", "[ClinicalAttachmentLevelMm] IS NULL OR ([ClinicalAttachmentLevelMm] >= 0 AND [ClinicalAttachmentLevelMm] <= 30)");
                    table.CheckConstraint("CK_PeriodontalSites_ProbingDepth", "[ProbingDepthMm] IS NULL OR ([ProbingDepthMm] >= 0 AND [ProbingDepthMm] <= 15)");
                    table.CheckConstraint("CK_PeriodontalSites_Recession", "[RecessionMm] IS NULL OR ([RecessionMm] >= 0 AND [RecessionMm] <= 15)");
                    table.ForeignKey(
                        name: "FK_PeriodontalSites_PeriodontalTeeth_PeriodontalToothId",
                        column: x => x.PeriodontalToothId,
                        principalTable: "PeriodontalTeeth",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PeriodontalExams_DoctorId",
                table: "PeriodontalExams",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodontalExams_PatientId_ExaminedAt",
                table: "PeriodontalExams",
                columns: new[] { "PatientId", "ExaminedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PeriodontalSites_PeriodontalToothId_Surface_Position",
                table: "PeriodontalSites",
                columns: new[] { "PeriodontalToothId", "Surface", "Position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PeriodontalTeeth_PeriodontalExamId_ToothNumber",
                table: "PeriodontalTeeth",
                columns: new[] { "PeriodontalExamId", "ToothNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PeriodontalSites");

            migrationBuilder.DropTable(
                name: "PeriodontalTeeth");

            migrationBuilder.DropTable(
                name: "PeriodontalExams");
        }
    }
}
