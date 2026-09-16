namespace Clinic_System.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("Logs/bootstrap-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

            try
            {
                Log.Information("Program Starting");

                var builder = WebApplication.CreateBuilder(args);
                builder.WebHost.ConfigureKestrel(options =>
                {
                    options.Limits.MaxRequestBodySize = FileClinicalAttachmentStorage.DefaultMaxFileBytes + (2 * 1024 * 1024);
                });

                // Serilog
                builder.Host.UseSerilog((context, services, config) =>
                {
                    config.ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services);
                });

                var connectionString = builder.Configuration.GetSection("constr").Value;

                builder.Services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseLazyLoadingProxies()
                        .UseSqlServer(connectionString)
                        .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
                });

                builder.Services.AddHangfireServices(connectionString);
                builder.Services.AddIdentityServices(builder.Configuration);
                builder.Services.AddPermissionAuthorization();
                builder.Services.AddSwaggerDocumentation();
                builder.Services.AddCorsPolicies();
                builder.Services.AddCustomRateLimiting(builder.Environment.IsDevelopment());
                builder.Services.AddSignalRServices();
                builder.Services.AddMessageBrokerServices(builder.Configuration);
                builder.Services.AddDataProtection();

                builder.Services.AddTransient<INotificationsService, NotificationsService>();
                builder.Services.AddPersistenceDependencies();
                builder.Services.AddApplicationDependencies();
                builder.Services.AddInfrastructureDependencies(builder.Configuration);
                builder.Services.AddSingleton<IClinicOperatingHoursService, FileClinicOperatingHoursService>();
                builder.Services.AddSingleton<IEmailSettingsProvider, FileClinicEmailSettingsService>();
                builder.Services.AddSingleton<IPatientNotificationSettingsService, FilePatientNotificationSettingsService>();
                builder.Services.AddSingleton<ILowStockEmailAlertSettingsService, FileLowStockEmailAlertSettingsService>();
                builder.Services.AddSingleton<ICampaignBookingNotifySettingsService, FileCampaignBookingNotifySettingsService>();
                builder.Services.AddSingleton<IOdontogramSymbolConfigService, FileOdontogramSymbolConfigService>();
                builder.Services.AddSingleton<IClinicalAttachmentStorage, FileClinicalAttachmentStorage>();

                builder.Services.AddHttpContextAccessor();
                builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
                {
                    options.MultipartBodyLengthLimit = FileClinicalAttachmentStorage.DefaultMaxFileBytes + (2 * 1024 * 1024);
                });
                builder.Services.AddControllers();

                var app = builder.Build();

                using (var scope = app.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    try
                    {
                        var context = services.GetRequiredService<AppDbContext>();
                        context.Database.Migrate();
                        await EnsureRequiredSchemaAsync(context);
                        await DevDataSeeder.SeedAsync(services);
                    }
                    catch (Exception ex)
                    {
                        var logger = services.GetRequiredService<ILogger<Program>>();
                        logger.LogError(ex, "An error occurred while migrating the database.");
                        try
                        {
                            var context = services.GetRequiredService<AppDbContext>();
                            await EnsureRequiredSchemaAsync(context);
                        }
                        catch (Exception ensureEx)
                        {
                            logger.LogError(ensureEx, "No se pudo completar el esquema mínimo de la base de datos.");
                        }
                    }
                }

                app.UseMiddleware<ErrorHandlerMiddleware>();
                app.UseMiddleware<BlacklistMiddleware>();

                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI(c =>
                    {
                        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Elite Clinic");
                    });
                }

                app.UseHttpsRedirection();


                app.UseCors("AllowAll");

                app.UseRouting();

                app.UseAuthentication();
                app.UseRateLimiter();
                app.UseAuthorization();


                app.MapControllers();

                app.MapHub<NotificationHub>("/hubs/notifications");

                app.UseHangfireDashboard();
                JobScheduler.ScheduleRecurringJobs(app);

                await app.RunAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Program Stoped");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static async Task EnsureRequiredSchemaAsync(AppDbContext context)
        {
            await context.Database.ExecuteSqlRawAsync("""
                IF COL_LENGTH(N'dbo.Doctors', N'SignatureImageUrl') IS NULL
                    ALTER TABLE [Doctors] ADD [SignatureImageUrl] nvarchar(max) NULL;

                IF COL_LENGTH(N'dbo.Doctors', N'CanViewAllClinicData') IS NULL
                    ALTER TABLE [Doctors] ADD [CanViewAllClinicData] bit NOT NULL
                        CONSTRAINT [DF_Doctors_CanViewAllClinicData] DEFAULT (1);

                IF COL_LENGTH(N'dbo.TreatmentProcedures', N'Target') IS NULL
                    ALTER TABLE [TreatmentProcedures] ADD [Target] int NOT NULL
                        CONSTRAINT [DF_TreatmentProcedures_Target] DEFAULT (1);

                IF COL_LENGTH(N'dbo.TreatmentProcedures', N'ToothKindFilter') IS NULL
                    ALTER TABLE [TreatmentProcedures] ADD [ToothKindFilter] int NOT NULL
                        CONSTRAINT [DF_TreatmentProcedures_ToothKindFilter] DEFAULT (0);

                IF COL_LENGTH(N'dbo.TreatmentProcedures', N'PricingMode') IS NULL
                    ALTER TABLE [TreatmentProcedures] ADD [PricingMode] int NOT NULL
                        CONSTRAINT [DF_TreatmentProcedures_PricingMode] DEFAULT (1);

                IF COL_LENGTH(N'dbo.Appointments', N'QuotedAmount') IS NULL
                    ALTER TABLE [Appointments] ADD [QuotedAmount] decimal(18,2) NULL;

                IF COL_LENGTH(N'dbo.Appointments', N'TreatmentProcedureId') IS NULL
                    ALTER TABLE [Appointments] ADD [TreatmentProcedureId] int NULL;

                IF COL_LENGTH(N'dbo.Appointments', N'PlanItemId') IS NULL
                    ALTER TABLE [Appointments] ADD [PlanItemId] int NULL;

                IF COL_LENGTH(N'dbo.Appointments', N'TreatmentPlanId') IS NULL
                    ALTER TABLE [Appointments] ADD [TreatmentPlanId] int NULL;

                IF COL_LENGTH(N'dbo.Appointments', N'ToothNumber') IS NULL
                    ALTER TABLE [Appointments] ADD [ToothNumber] int NULL;

                IF COL_LENGTH(N'dbo.Appointments', N'AttendanceLinkRespondedAt') IS NULL
                    ALTER TABLE [Appointments] ADD [AttendanceLinkRespondedAt] datetime2 NULL;

                IF COL_LENGTH(N'dbo.Appointments', N'AttendanceLinkAccepted') IS NULL
                    ALTER TABLE [Appointments] ADD [AttendanceLinkAccepted] bit NULL;

                IF COL_LENGTH(N'dbo.Appointments', N'AttendanceLinkComment') IS NULL
                    ALTER TABLE [Appointments] ADD [AttendanceLinkComment] nvarchar(500) NULL;
                """);

            await context.Database.ExecuteSqlRawAsync("""
                IF COL_LENGTH(N'dbo.TreatmentProcedures', N'PricingMode') IS NOT NULL
                   AND NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_TreatmentProcedures_PricingMode')
                    ALTER TABLE [TreatmentProcedures] ADD CONSTRAINT [CK_TreatmentProcedures_PricingMode]
                        CHECK ([PricingMode] IN (0, 1, 2));
                """);

            await EnsureClinicalAttachmentsTableAsync(context);
            await EnsurePatientInformedConsentsTableAsync(context);
            await EnsureCampaignAppointmentRequestsTableAsync(context);
            await EnsurePatientMedicalCertificatesTableAsync(context);
        }

        private static async Task EnsurePatientInformedConsentsTableAsync(AppDbContext context)
        {
            await context.Database.ExecuteSqlRawAsync("""
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

                IF COL_LENGTH(N'dbo.PatientInformedConsents', N'ProcedureExplanation') IS NOT NULL
                   AND EXISTS (
                        SELECT 1 FROM sys.columns
                        WHERE object_id = OBJECT_ID(N'dbo.PatientInformedConsents')
                          AND name = N'ProcedureExplanation'
                          AND max_length < 16000
                   )
                    ALTER TABLE [PatientInformedConsents] ALTER COLUMN [ProcedureExplanation] nvarchar(8000) NOT NULL;
                """);
        }

        private static async Task EnsurePatientMedicalCertificatesTableAsync(AppDbContext context)
        {
            await context.Database.ExecuteSqlRawAsync("""
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
                        CONSTRAINT [FK_PatientMedicalCertificates_Patients_PatientId]
                            FOREIGN KEY ([PatientId]) REFERENCES [Patients] ([Id]),
                        CONSTRAINT [FK_PatientMedicalCertificates_Doctors_DoctorId]
                            FOREIGN KEY ([DoctorId]) REFERENCES [Doctors] ([Id]) ON DELETE SET NULL
                    );

                    CREATE INDEX [IX_PatientMedicalCertificates_DoctorId]
                        ON [PatientMedicalCertificates] ([DoctorId]);

                    CREATE INDEX [IX_PatientMedicalCertificates_PatientId_IssuedAt]
                        ON [PatientMedicalCertificates] ([PatientId], [IssuedAt]);
                END
                """);
        }

        private static async Task EnsureClinicalAttachmentsTableAsync(AppDbContext context)
        {
            await context.Database.ExecuteSqlRawAsync("""
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

        private static async Task EnsureCampaignAppointmentRequestsTableAsync(AppDbContext context)
        {
            await context.Database.ExecuteSqlRawAsync("""
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
    }
}
//{
//    "emailOrUserName": "dr.ahmed@clinic.com",
//  "password": "Doctor@123"
//}