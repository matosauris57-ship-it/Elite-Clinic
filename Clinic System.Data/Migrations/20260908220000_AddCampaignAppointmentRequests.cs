using Clinic_System.Data.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinic_System.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260908220000_AddCampaignAppointmentRequests")]
    public partial class AddCampaignAppointmentRequests : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'dbo.CampaignAppointmentRequests', N'U') IS NULL
                BEGIN
                    CREATE TABLE [CampaignAppointmentRequests] (
                        [Id] int NOT NULL IDENTITY,
                        [EmailCampaignId] int NOT NULL,
                        [EmailCampaignRecipientId] int NOT NULL,
                        [PatientId] int NOT NULL,
                        [RequestedDoctorId] int NOT NULL,
                        [RequestedAt] datetime2 NOT NULL,
                        [RequestedAppointmentAt] datetime2 NOT NULL,
                        [TreatmentProcedureId] int NULL,
                        [OtherServiceText] nvarchar(200) NULL,
                        [Status] nvarchar(20) NOT NULL,
                        [AppointmentId] int NULL,
                        [ScheduledDoctorId] int NULL,
                        [ScheduledAppointmentAt] datetime2 NULL,
                        [ResolvedAt] datetime2 NULL,
                        [ResolvedByUserId] nvarchar(450) NULL,
                        [ResolvedByName] nvarchar(256) NULL,
                        [StaffNote] nvarchar(500) NULL,
                        [PatientNotified] bit NOT NULL CONSTRAINT [DF_CampaignAppointmentRequests_PatientNotified] DEFAULT (0),
                        CONSTRAINT [PK_CampaignAppointmentRequests] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_CampaignAppointmentRequests_EmailCampaigns]
                            FOREIGN KEY ([EmailCampaignId]) REFERENCES [EmailCampaigns] ([Id]),
                        CONSTRAINT [FK_CampaignAppointmentRequests_EmailCampaignRecipients]
                            FOREIGN KEY ([EmailCampaignRecipientId]) REFERENCES [EmailCampaignRecipients] ([Id]),
                        CONSTRAINT [FK_CampaignAppointmentRequests_Patients]
                            FOREIGN KEY ([PatientId]) REFERENCES [Patients] ([Id]),
                        CONSTRAINT [FK_CampaignAppointmentRequests_RequestedDoctors]
                            FOREIGN KEY ([RequestedDoctorId]) REFERENCES [Doctors] ([Id]),
                        CONSTRAINT [FK_CampaignAppointmentRequests_ScheduledDoctors]
                            FOREIGN KEY ([ScheduledDoctorId]) REFERENCES [Doctors] ([Id]),
                        CONSTRAINT [FK_CampaignAppointmentRequests_TreatmentProcedures]
                            FOREIGN KEY ([TreatmentProcedureId]) REFERENCES [TreatmentProcedures] ([Id]) ON DELETE SET NULL,
                        CONSTRAINT [FK_CampaignAppointmentRequests_Appointments]
                            FOREIGN KEY ([AppointmentId]) REFERENCES [Appointments] ([Id]) ON DELETE SET NULL
                    );

                    CREATE UNIQUE INDEX [IX_CampaignAppointmentRequests_Recipient]
                        ON [CampaignAppointmentRequests] ([EmailCampaignRecipientId]);

                    CREATE INDEX [IX_CampaignAppointmentRequests_Status_Requested]
                        ON [CampaignAppointmentRequests] ([Status], [RequestedAt]);
                END
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "CampaignAppointmentRequests");
        }
    }
}
