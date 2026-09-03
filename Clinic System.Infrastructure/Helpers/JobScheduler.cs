namespace Clinic_System.Infrastructure.Helpers
{
    public static class JobScheduler
    {
        // ميثود واحدة بتلم كل الـ Recurring Jobs
        public static void ScheduleRecurringJobs(IApplicationBuilder app)
        {
            // بنستخدم الـ ServiceProvider عشان نجيب الـ JobManager
            using var scope = app.ApplicationServices.CreateScope();
            var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

            // ==========================================
            // Job 1: إلغاء المواعيد المتأخرة (كل ساعة)
            // ==========================================
            recurringJobManager.AddOrUpdate<IAppointmentService>(
                "cancel-overdue-appointments",
                service => service.CancelOverdueAppointmentsAsync(),
                Cron.Hourly
            );

            // ==========================================
            // Job 2: تنظيف التوكنات المنتهية (كل يوم) - الجديد
            // ==========================================
            recurringJobManager.AddOrUpdate<IRefreshTokenCleanupService>(
                "cleanup-tokens",
                service => service.RemoveExpiredRefreshTokensAsync(),
                Cron.Daily
            );

            recurringJobManager.AddOrUpdate<IPatientNotificationDispatchService>(
                "patient-email-notifications",
                service => service.DispatchDueAsync(),
                "*/15 * * * *"
            );

            recurringJobManager.AddOrUpdate<IEmailCampaignService>(
                "patient-email-campaigns",
                service => service.DispatchDueAsync(),
                "*/15 * * * *"
            );
        }
    }
}
