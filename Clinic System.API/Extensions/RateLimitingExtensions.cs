namespace Clinic_System.API.Extensions
{
    public static class RateLimitingExtensions
    {
        private const int ProdAuthenticatedPermitLimit = 100;
        private const int DevAuthenticatedPermitLimit = 1000;
        private const int ProdAnonymousPermitLimit = 60;
        private const int DevAnonymousPermitLimit = 300;
        private const string RateLimitedMessage = "Demasiadas solicitudes. Espera un momento e inténtalo de nuevo.";

        public static IServiceCollection AddCustomRateLimiting(this IServiceCollection services, bool isDevelopment = false)
        {
            var authPermitLimit = isDevelopment ? 100 : 5;
            var authenticatedPermitLimit = isDevelopment ? DevAuthenticatedPermitLimit : ProdAuthenticatedPermitLimit;
            var anonymousPermitLimit = isDevelopment ? DevAnonymousPermitLimit : ProdAnonymousPermitLimit;

            services.AddRateLimiter(options =>
            {
                options.OnRejected = async (context, token) =>
                {
                    var ip = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown_IP";
                    var path = context.HttpContext.Request.Path.ToString();
                    var method = context.HttpContext.Request.Method;
                    var userId = context.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "Guest";

                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();

                    logger.LogWarning("Rate Limit Exceeded! IP: {IP}, User: {UserId}, Tried to attack: {Method} {Path}", ip, userId, method, path);

                    if (!isDevelopment)
                    {
                        var cacheService = context.HttpContext.RequestServices.GetRequiredService<ICacheService>();
                        string cacheKey = $"Blacklist_IP_{ip}";

                        var crimeData = new
                        {
                            AttackerIP = ip,
                            AttackerId = userId,
                            TargetEndpoint = path,
                            AttackTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
                        };

                        await cacheService.SetDataAsync(cacheKey, crimeData, TimeSpan.FromHours(24));
                    }

                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.HttpContext.Response.ContentType = "application/json";

                    var errorResponse = new
                    {
                        message = RateLimitedMessage,
                        statusCode = 429
                    };

                    await context.HttpContext.Response.WriteAsJsonAsync(errorResponse, token);
                };

                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                {
                    var isAuthenticated = context.User.Identity?.IsAuthenticated ?? false;

                    if (isAuthenticated)
                    {
                        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                     ?? context.User.Identity?.Name
                                     ?? "unknown_user";

                        return RateLimitPartition.GetSlidingWindowLimiter(
                            partitionKey: $"User_{userId}",
                            factory: _ => new SlidingWindowRateLimiterOptions
                            {
                                PermitLimit = authenticatedPermitLimit,
                                Window = TimeSpan.FromMinutes(1),
                                SegmentsPerWindow = 6,
                                QueueLimit = 0,
                                AutoReplenishment = true
                            });
                    }

                    var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown_ip";
                    return RateLimitPartition.GetSlidingWindowLimiter(
                        partitionKey: $"IP_{ip}",
                        factory: _ => new SlidingWindowRateLimiterOptions
                        {
                            PermitLimit = anonymousPermitLimit,
                            Window = TimeSpan.FromMinutes(1),
                            SegmentsPerWindow = 6,
                            QueueLimit = 0,
                            AutoReplenishment = true
                        });
                });

                options.AddPolicy("AuthLimiter", httpContext =>
                    RateLimitPartition.GetSlidingWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown_IP",
                        factory: _ => new SlidingWindowRateLimiterOptions
                        {
                            PermitLimit = authPermitLimit,
                            Window = TimeSpan.FromMinutes(1),
                            SegmentsPerWindow = 3,
                            QueueLimit = 0,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            AutoReplenishment = true
                        }));
            });

            return services;
        }
    }
}
