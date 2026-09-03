namespace Clinic_System.API.Extensions
{
    public static class IdentityServiceExtensions
    {
        // لاحظ هنا إننا بعتنا الـ IConfiguration عشان نقدر نقرأ من ملف الـ appsettings
        public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
        {
            // ==========================================
            // 1. Identity Configuration
            // ==========================================
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Password settings
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;

                // User settings
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            // ==========================================
            // 2. JWT Configuration & Validation Checks
            // ==========================================
            var jwtSettings = configuration.GetSection("JWT").Get<JwtSettings>();

            // نقلنا الـ Exceptions هنا عشان ننضف الـ Program.cs خالص!
            if (jwtSettings == null || string.IsNullOrWhiteSpace(jwtSettings.SecritKey))
                throw new Exception("JWT SecretKey is missing in appsettings.json");

            if (string.IsNullOrWhiteSpace(jwtSettings.IssuerIP))
                throw new Exception("JWT IssuerIP is missing in appsettings.json");

            if (string.IsNullOrWhiteSpace(jwtSettings.AudienceIP))
                throw new Exception("JWT AudienceIP is missing in appsettings.json");

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.MapInboundClaims = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.IssuerIP,
                    ValidAudience = jwtSettings.AudienceIP,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecritKey)),
                    NameClaimType = ClaimTypes.Name,
                    RoleClaimType = ClaimTypes.Role
                };

                // SignalR (WebSockets/SSE) envía el JWT en ?access_token=...
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

            return services;
        }
    }
}