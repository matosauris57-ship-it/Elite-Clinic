using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinic_System.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddClinicalOdontogramTimeline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ToothSurface",
                table: "PlanItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TreatmentProcedureId",
                table: "PlanItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ToothSurface",
                table: "DentalTreatments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TreatmentProcedureId",
                table: "DentalTreatments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DentalClinicalEvents",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    ToothNumber = table.Column<int>(type: "int", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Phase = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ReferenceType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ReferenceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RecordedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DentalClinicalEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DentalClinicalEvents_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ToothChartEntries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    ToothNumber = table.Column<int>(type: "int", nullable: false),
                    Surface = table.Column<int>(type: "int", nullable: false),
                    Phase = table.Column<int>(type: "int", nullable: false),
                    Condition = table.Column<int>(type: "int", nullable: false),
                    Severity = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AppointmentId = table.Column<int>(type: "int", nullable: true),
                    RecordedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToothChartEntries", x => x.Id);
                    table.CheckConstraint("CK_ToothChartEntries_FDI", "(([ToothNumber] BETWEEN 11 AND 18) OR ([ToothNumber] BETWEEN 21 AND 28) OR ([ToothNumber] BETWEEN 31 AND 38) OR ([ToothNumber] BETWEEN 41 AND 48) OR ([ToothNumber] BETWEEN 51 AND 55) OR ([ToothNumber] BETWEEN 61 AND 65) OR ([ToothNumber] BETWEEN 71 AND 75) OR ([ToothNumber] BETWEEN 81 AND 85))");
                    table.ForeignKey(
                        name: "FK_ToothChartEntries_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ToothChartEntries_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(
                """
                INSERT INTO [ToothChartEntries]
                    ([PatientId], [ToothNumber], [Surface], [Phase], [Condition], [Severity],
                     [Notes], [AppointmentId], [RecordedByUserId], [RecordedAt])
                SELECT
                    [PatientId], [ToothNumber], 0, 0, [DiagnosisCondition], NULL,
                    [Notes], NULL, NULL, COALESCE([UpdatedAt], [CreatedAt], SYSUTCDATETIME())
                FROM [ToothRecords]
                WHERE [IsDeleted] = 0
                  AND (([ToothNumber] BETWEEN 11 AND 18) OR ([ToothNumber] BETWEEN 21 AND 28)
                    OR ([ToothNumber] BETWEEN 31 AND 38) OR ([ToothNumber] BETWEEN 41 AND 48)
                    OR ([ToothNumber] BETWEEN 51 AND 55) OR ([ToothNumber] BETWEEN 61 AND 65)
                    OR ([ToothNumber] BETWEEN 71 AND 75) OR ([ToothNumber] BETWEEN 81 AND 85));

                INSERT INTO [ToothChartEntries]
                    ([PatientId], [ToothNumber], [Surface], [Phase], [Condition], [Severity],
                     [Notes], [AppointmentId], [RecordedByUserId], [RecordedAt])
                SELECT
                    [PatientId], [ToothNumber], 0, 2, [TreatmentCondition], NULL,
                    [Notes], NULL, NULL, COALESCE([UpdatedAt], [CreatedAt], SYSUTCDATETIME())
                FROM [ToothRecords]
                WHERE [IsDeleted] = 0 AND [TreatmentCondition] IS NOT NULL
                  AND (([ToothNumber] BETWEEN 11 AND 18) OR ([ToothNumber] BETWEEN 21 AND 28)
                    OR ([ToothNumber] BETWEEN 31 AND 38) OR ([ToothNumber] BETWEEN 41 AND 48)
                    OR ([ToothNumber] BETWEEN 51 AND 55) OR ([ToothNumber] BETWEEN 61 AND 65)
                    OR ([ToothNumber] BETWEEN 71 AND 75) OR ([ToothNumber] BETWEEN 81 AND 85));

                INSERT INTO [DentalClinicalEvents]
                    ([PatientId], [ToothNumber], [Type], [Phase], [Title], [Description],
                     [ReferenceType], [ReferenceId], [RecordedByUserId], [RecordedAt])
                SELECT
                    [PatientId], [ToothNumber], 0, 0,
                    CONCAT(N'Migración de odontograma: diente ', [ToothNumber]),
                    [Notes], N'ToothRecord', CONVERT(nvarchar(100), [Id]), NULL,
                    COALESCE([UpdatedAt], [CreatedAt], SYSUTCDATETIME())
                FROM [ToothRecords]
                WHERE [IsDeleted] = 0
                  AND (([ToothNumber] BETWEEN 11 AND 18) OR ([ToothNumber] BETWEEN 21 AND 28)
                    OR ([ToothNumber] BETWEEN 31 AND 38) OR ([ToothNumber] BETWEEN 41 AND 48)
                    OR ([ToothNumber] BETWEEN 51 AND 55) OR ([ToothNumber] BETWEEN 61 AND 65)
                    OR ([ToothNumber] BETWEEN 71 AND 75) OR ([ToothNumber] BETWEEN 81 AND 85));
                """);

            migrationBuilder.CreateIndex(
                name: "IX_PlanItems_TreatmentProcedureId",
                table: "PlanItems",
                column: "TreatmentProcedureId");

            migrationBuilder.CreateIndex(
                name: "IX_DentalTreatments_TreatmentProcedureId",
                table: "DentalTreatments",
                column: "TreatmentProcedureId");

            migrationBuilder.CreateIndex(
                name: "IX_DentalClinicalEvents_PatientId_RecordedAt",
                table: "DentalClinicalEvents",
                columns: new[] { "PatientId", "RecordedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_DentalClinicalEvents_PatientId_ToothNumber_RecordedAt",
                table: "DentalClinicalEvents",
                columns: new[] { "PatientId", "ToothNumber", "RecordedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ToothChartEntries_AppointmentId",
                table: "ToothChartEntries",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ToothChartEntries_PatientId_ToothNumber_Surface_Phase_RecordedAt",
                table: "ToothChartEntries",
                columns: new[] { "PatientId", "ToothNumber", "Surface", "Phase", "RecordedAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_DentalTreatments_TreatmentProcedures_TreatmentProcedureId",
                table: "DentalTreatments",
                column: "TreatmentProcedureId",
                principalTable: "TreatmentProcedures",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PlanItems_TreatmentProcedures_TreatmentProcedureId",
                table: "PlanItems",
                column: "TreatmentProcedureId",
                principalTable: "TreatmentProcedures",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DentalTreatments_TreatmentProcedures_TreatmentProcedureId",
                table: "DentalTreatments");

            migrationBuilder.DropForeignKey(
                name: "FK_PlanItems_TreatmentProcedures_TreatmentProcedureId",
                table: "PlanItems");

            migrationBuilder.DropTable(
                name: "DentalClinicalEvents");

            migrationBuilder.DropTable(
                name: "ToothChartEntries");

            migrationBuilder.DropIndex(
                name: "IX_PlanItems_TreatmentProcedureId",
                table: "PlanItems");

            migrationBuilder.DropIndex(
                name: "IX_DentalTreatments_TreatmentProcedureId",
                table: "DentalTreatments");

            migrationBuilder.DropColumn(
                name: "ToothSurface",
                table: "PlanItems");

            migrationBuilder.DropColumn(
                name: "TreatmentProcedureId",
                table: "PlanItems");

            migrationBuilder.DropColumn(
                name: "ToothSurface",
                table: "DentalTreatments");

            migrationBuilder.DropColumn(
                name: "TreatmentProcedureId",
                table: "DentalTreatments");
        }
    }
}
