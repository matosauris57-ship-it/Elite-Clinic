namespace Clinic_System.API.Middlewares
{
    public class BlacklistMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<BlacklistMiddleware> _logger;

        // 1. الـ Constructor: بياخد الـ next (الخطوة الجاية في الـ Pipeline) والـ Logger
        public BlacklistMiddleware(RequestDelegate next, ILogger<BlacklistMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        // 2. دالة InvokeAsync: دي اللي بتتنفذ مع كل ريكويست بيدخل السيرفر
        // لاحظ: حقنّا الـ ICacheService هنا مش في الـ Constructor، لأن الـ Cache Scoped والـ Middleware Singleton
        public async Task InvokeAsync(HttpContext context, ICacheService cacheService)
        {
            // 3. بنجيب الـ IP بتاع الراجل اللي بيحاول يدخل
            var ip = context.Connection.RemoteIpAddress?.ToString();

            if (!string.IsNullOrEmpty(ip))
            {
                // 4. بنجهز اسم المفتاح اللي بندور عليه في الريديس
                string cacheKey = $"Blacklist_IP_{ip}";

                // 5. بنسأل الريديس: هل الـ IP ده موجود عندك؟
                // (استخدمنا GetAsync أياً كان نوع الداتا اللي بترجع، المهم هل هي موجودة ولا لأ)
                var isBlacklisted = await cacheService.GetDataAsync<object>(cacheKey);

                // 6. لو موجود في الريديس (يعني البودي جارد لقاه في البلاك ليست)
                if (isBlacklisted != null)
                {
                    _logger.LogWarning("Blocked request from blacklisted IP: {IP}", ip);

                    // بنوقف الريكويست فوراً ونرجع إيرور 403 (ممنوع الدخول)
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    context.Response.ContentType = "application/json";

                    var response = new
                    {
                        message = "Your IP is currently banned due to suspicious activity. Please contact support."
                    };

                    await context.Response.WriteAsJsonAsync(response);

                    // السر هنا: الـ return دي بتنهي الرحلة، والريكويست مش هيكمل للـ Controllers
                    return;
                }
            }

            // 7. لو الراجل سليم ومش في البلاك ليست، بنقوله "اتفضل كمل طريقك" (بننادي الـ next)
            await _next(context);
        }
    }
}