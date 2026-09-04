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
                builder.Services.AddSingleton<IOdontogramSymbolConfigService, FileOdontogramSymbolConfigService>();

                builder.Services.AddHttpContextAccessor();
                builder.Services.AddControllers();

                var app = builder.Build();

                using (var scope = app.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    try
                    {
                        var context = services.GetRequiredService<AppDbContext>();
                        context.Database.Migrate();
                        await DevDataSeeder.SeedAsync(services);
                    }
                    catch (Exception ex)
                    {
                        var logger = services.GetRequiredService<ILogger<Program>>();
                        logger.LogError(ex, "An error occurred while migrating the database.");
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
    }
}
//{
//    "emailOrUserName": "dr.ahmed@clinic.com",
//  "password": "Doctor@123"
//}