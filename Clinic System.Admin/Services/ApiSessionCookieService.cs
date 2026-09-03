using System.Text.Json;
using DentalCare.Admin.Models;
using Microsoft.AspNetCore.DataProtection;

namespace DentalCare.Admin.Services;

public class ApiSessionCookieService
{
    public const string CookieName = "dentalcare_api_session";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IDataProtector _protector;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApiSessionCookieService(IDataProtectionProvider dataProtectionProvider, IHttpContextAccessor httpContextAccessor)
    {
        _protector = dataProtectionProvider.CreateProtector("DentalCare.Admin.ApiSession.v1");
        _httpContextAccessor = httpContextAccessor;
    }

    public void Write(SessionBootstrapData data)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null || httpContext.Response.HasStarted)
            return;

        var payload = _protector.Protect(JsonSerializer.Serialize(data, JsonOptions));
        httpContext.Response.Cookies.Append(CookieName, payload, new CookieOptions
        {
            HttpOnly = true,
            Secure = httpContext.Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            IsEssential = true,
            Expires = DateTimeOffset.UtcNow.AddHours(8)
        });
    }

    public void Delete()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null || httpContext.Response.HasStarted)
            return;

        var options = new CookieOptions
        {
            HttpOnly = true,
            Secure = httpContext.Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Expires = DateTimeOffset.UnixEpoch
        };

        httpContext.Response.Cookies.Delete(CookieName, options);
        httpContext.Response.Cookies.Append(CookieName, string.Empty, options);
    }

    public SessionBootstrapData? TryRead(HttpContext? httpContext = null)
    {
        httpContext ??= _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return null;

        if (!httpContext.Request.Cookies.TryGetValue(CookieName, out var payload) || string.IsNullOrWhiteSpace(payload))
            return null;

        try
        {
            var json = _protector.Unprotect(payload);
            return JsonSerializer.Deserialize<SessionBootstrapData>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }
}
