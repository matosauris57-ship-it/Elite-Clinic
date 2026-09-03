namespace Clinic_System.Infrastructure
{
    public static class InfrastructureRegistration
    {
        public static IServiceCollection AddInfrastructureDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            // Email Settings
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

            // Services
            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IAuthorizationService, AuthorizationService>();
            services.AddScoped<IPermissionResolver, PermissionResolver>();
            services.AddScoped<IRoleManagementService, RoleManagementService>();
            services.AddScoped<IUserManagementService, UserManagementService>();
            services.AddScoped<IRefreshTokenCleanupService, RefreshTokenCleanupService>();
            services.AddScoped<IBackgroundJobService, HangfireBackgroundJobService>();
            services.AddScoped<IAppointmentNotificationService, AppointmentEmailNotificationService>();
            services.AddScoped<IIdentityNotificationService, IdentityNotificationService>();
            services.AddTransient<IEmailService, EmailService>();
            services.AddScoped<IPatientNotificationDispatchService, PatientNotificationDispatchService>();
            services.AddScoped<IEmailCampaignService, EmailCampaignService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IGoogleAuthService, GoogleAuthService>();
            services.Configure<ClinicSettings>(configuration.GetSection("ClinicSettings"));

            // بيقرأ القسم من الـ JSON ويربطه بالكلاس
            services.Configure<JwtSettings>(configuration.GetSection("JWT"));


            var redisConnectionString = configuration.GetSection("Redis:ConnectionString").Value;

            if (!string.IsNullOrWhiteSpace(redisConnectionString))
            {
                services.AddScoped<IDistributedLockService, RedisDistributedLockService>();
                services.AddScoped<ICacheService, RedisCacheService>();

                var redisConfiguration = ConfigurationOptions.Parse(redisConnectionString);
                redisConfiguration.ReconnectRetryPolicy = new ExponentialRetry(500, 2000);
                services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConfiguration));
            }
            else
            {
                services.AddScoped<IDistributedLockService, NullDistributedLockService>();
                services.AddScoped<ICacheService, NullCacheService>();
            }

            return services;
        }
    }
}
