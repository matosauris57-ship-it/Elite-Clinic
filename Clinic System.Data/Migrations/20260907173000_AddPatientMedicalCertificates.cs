using Clinic_System.Data.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinic_System.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260907173000_AddPatientMedicalCertificates")]
    public partial class AddPatientMedicalCertificates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'dbo.PatientMedicalCertificates', N'U') IS NULL
                BEGIN
                    CREATE TABLE [PatientMedicalCertificates] (
                        [Id] int NOT NULL IDENTITY,
                        [PatientId] int NOT NULL,
                        [DoctorId] int NULL,
                        [IssuedAt] datetime2 NOT NULL,
                        [CertificateType] nvarchar(120) NOT NULL,
                        [Diagnosis] nvarchar(1000) NOT NULL,
                        [Recommendation] nvarchar(2000) NOT NULL,
                        [IncludesRest] bit NOT NULL,
                        [RestStartDate] datetime2 NULL,
                        [RestEndDate] datetime2 NULL,
                        [RestDays] int NULL,
                        [Observations] nvarchar(2000) NULL,
                        [Purpose] nvarchar(80) NULL,
                        [RecordedByUserId] nvarchar(450) NULL,
                        [IsDeleted] bit NOT NULL,
                        [DeletedAt] datetime2 NULL,
                        [CreatedAt] datetime2 NOT NULL,
                        [UpdatedAt] datetime2 NULL,
                        CONSTRAINT [PK_PatientMedicalCertificates] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_PatientMedicalCertificates_Doctors_DoctorId]
                            FOREIGN KEY ([DoctorId]) REFERENCES [Doctors] ([Id]) ON DELETE SET NULL,
                        CONSTRAINT [FK_PatientMedicalCertificates_Patients_PatientId]
                            FOREIGN KEY ([PatientId]) REFERENCES [Patients] ([Id])
                    );

                    CREATE INDEX [IX_PatientMedicalCertificates_DoctorId]
                        ON [PatientMedicalCertificates] ([DoctorId]);

                    CREATE INDEX [IX_PatientMedicalCertificates_PatientId_IssuedAt]
                        ON [PatientMedicalCertificates] ([PatientId], [IssuedAt]);
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'dbo.PatientMedicalCertificates', N'U') IS NOT NULL
                    DROP TABLE [PatientMedicalCertificates];
                """);
        }
    }
}
