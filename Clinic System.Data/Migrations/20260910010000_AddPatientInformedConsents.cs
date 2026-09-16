using Clinic_System.Data.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinic_System.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260910010000_AddPatientInformedConsents")]
    public partial class AddPatientInformedConsents : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'dbo.PatientInformedConsents', N'U') IS NULL
                BEGIN
                    CREATE TABLE [PatientInformedConsents] (
                        [Id] int NOT NULL IDENTITY,
                        [PatientId] int NOT NULL,
                        [ConsentType] int NOT NULL,
                        [Title] nvarchar(200) NOT NULL,
                        [ProcedureExplanation] nvarchar(8000) NOT NULL,
                        [Benefits] nvarchar(2000) NOT NULL,
                        [Risks] nvarchar(2000) NOT NULL,
                        [Alternatives] nvarchar(2000) NOT NULL,
                        [AuthorizationText] nvarchar(1000) NOT NULL,
                        [Notes] nvarchar(1000) NULL,
                        [ToothNumber] int NULL,
                        [DoctorId] int NULL,
                        [SignedOn] datetime2 NOT NULL,
                        [OriginalFileName] nvarchar(260) NOT NULL,
                        [StoredFileName] nvarchar(80) NOT NULL,
                        [ContentType] nvarchar(120) NOT NULL,
                        [FileSizeBytes] bigint NOT NULL,
                        [RecordedByUserId] nvarchar(450) NULL,
                        [IsDeleted] bit NOT NULL,
                        [DeletedAt] datetime2 NULL,
                        [CreatedAt] datetime2 NOT NULL,
                        [UpdatedAt] datetime2 NULL,
                        CONSTRAINT [PK_PatientInformedConsents] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_PatientInformedConsents_Patients_PatientId]
                            FOREIGN KEY ([PatientId]) REFERENCES [Patients] ([Id]) ON DELETE CASCADE,
                        CONSTRAINT [FK_PatientInformedConsents_Doctors_DoctorId]
                            FOREIGN KEY ([DoctorId]) REFERENCES [Doctors] ([Id]) ON DELETE SET NULL
                    );

                    CREATE INDEX [IX_PatientInformedConsents_PatientId]
                        ON [PatientInformedConsents] ([PatientId]);

                    CREATE INDEX [IX_PatientInformedConsents_Patient_Type_Signed]
                        ON [PatientInformedConsents] ([PatientId], [ConsentType], [SignedOn]);

                    CREATE INDEX [IX_PatientInformedConsents_DoctorId]
                        ON [PatientInformedConsents] ([DoctorId]);
                END
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PatientInformedConsents");
        }
    }
}
