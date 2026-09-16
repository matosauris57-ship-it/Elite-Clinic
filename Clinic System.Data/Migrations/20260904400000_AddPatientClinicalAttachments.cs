using Clinic_System.Data.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinic_System.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260904400000_AddPatientClinicalAttachments")]
    public partial class AddPatientClinicalAttachments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'dbo.PatientClinicalAttachments', N'U') IS NULL
                BEGIN
                    CREATE TABLE [PatientClinicalAttachments] (
                        [Id] int NOT NULL IDENTITY,
                        [PatientId] int NOT NULL,
                        [Kind] int NOT NULL,
                        [Title] nvarchar(200) NOT NULL,
                        [Subtype] nvarchar(80) NULL,
                        [Notes] nvarchar(1000) NULL,
                        [ToothNumber] int NULL,
                        [CapturedOn] datetime2 NULL,
                        [OriginalFileName] nvarchar(260) NOT NULL,
                        [StoredFileName] nvarchar(80) NOT NULL,
                        [ContentType] nvarchar(120) NOT NULL,
                        [FileSizeBytes] bigint NOT NULL,
                        [RecordedByUserId] nvarchar(450) NULL,
                        [IsDeleted] bit NOT NULL,
                        [DeletedAt] datetime2 NULL,
                        [CreatedAt] datetime2 NOT NULL,
                        [UpdatedAt] datetime2 NULL,
                        CONSTRAINT [PK_PatientClinicalAttachments] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_PatientClinicalAttachments_Patients_PatientId]
                            FOREIGN KEY ([PatientId]) REFERENCES [Patients] ([Id]) ON DELETE CASCADE
                    );

                    CREATE INDEX [IX_PatientClinicalAttachments_PatientId]
                        ON [PatientClinicalAttachments] ([PatientId]);

                    CREATE INDEX [IX_PatientClinicalAttachments_Patient_Kind_Created]
                        ON [PatientClinicalAttachments] ([PatientId], [Kind], [CreatedAt]);
                END
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PatientClinicalAttachments");
        }
    }
}
