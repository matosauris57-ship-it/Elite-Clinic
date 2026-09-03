using System.Security.Claims;
using DentalCare.Admin.Components;
using Microsoft.AspNetCore.Components.Server.Circuits;
using DentalCare.Admin.Models;
using DentalCare.Admin.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));
builder.Services.Configure<ClinicSettings>(builder.Configuration.GetSection("ClinicSettings"));

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMemoryCache();
builder.Services.AddDataProtection();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ApiSessionCookieService>();
builder.Services.AddScoped<CircuitSessionContext>();
builder.Services.AddScoped<SessionReadyService>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
    });
builder.Services.AddAuthorization();
builder.Services.AddAdminPermissionAuthorization();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddScoped<TokenStorage>();
builder.Services.AddScoped<CircuitHandler, SessionCircuitHandler>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<ApiTokenRefreshService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<DashboardLayoutApiService>();
builder.Services.AddScoped<AppointmentBookingService>();
builder.Services.AddScoped<DoctorMaintenanceService>();
builder.Services.AddScoped<MedicalConditionMaintenanceService>();
builder.Services.AddScoped<PatientMaintenanceService>();
builder.Services.AddScoped<ToothChartService>();
        builder.Services.AddScoped<PeriodontalExamService>();
        builder.Services.AddScoped<PatientPrescriptionService>();
builder.Services.AddScoped<TreatmentProcedureMaintenanceService>();
builder.Services.AddScoped<ClinicalTreatmentMaintenanceService>();
builder.Services.AddScoped<TreatmentPlanMaintenanceService>();
builder.Services.AddScoped<AgendaMaintenanceService>();
builder.Services.AddScoped<WaitingRoomHubService>();
builder.Services.AddScoped<BillingMaintenanceService>();
builder.Services.AddScoped<PermissionService>();
builder.Services.AddScoped<AccessControlMaintenanceService>();
builder.Services.AddSingleton<ClinicProfileService>();
builder.Services.AddSingleton<WhatsAppMessageSettingsService>();
builder.Services.AddSingleton<EmailMessageSettingsService>();
builder.Services.AddScoped<EmailCampaignMaintenanceService>();
builder.Services.AddSingleton<OdontogramSymbolConfigState>();
builder.Services.AddSingleton<LoadingMediaCatalog>();
builder.Services.AddScoped<OdontogramSymbolConfigService>();
builder.Services.AddHostedService<ApiAvailabilityHostedService>();

var apiUrl = builder.Configuration["ApiSettings:ApiBaseUrl"] ?? "http://localhost:5129";

builder.Services.AddHttpClient("ClinicApiLogin", client =>
{
    client.BaseAddress = new Uri(apiUrl);
});

builder.Services.AddScoped<AuthenticatedApiClient>(sp =>
{
    var tokenStorage = sp.GetRequiredService<TokenStorage>();
    var circuitSession = sp.GetRequiredService<CircuitSessionContext>();
    var sessionReady = sp.GetRequiredService<SessionReadyService>();

    HttpMessageHandler innerHandler = new HttpClientHandler();
    if (builder.Environment.IsDevelopment() && apiUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
    {
        innerHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
    }

    var authHandler = new ApiAuthHandler(
        tokenStorage,
        circuitSession,
        sessionReady,
        sp.GetRequiredService<IHttpContextAccessor>(),
        sp.GetRequiredService<ApiTokenRefreshService>())
    {
        InnerHandler = innerHandler
    };

    return new AuthenticatedApiClient(new HttpClient(authHandler)
    {
        BaseAddress = new Uri(apiUrl)
    });
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapGet("/auth/complete", async (string key, IMemoryCache cache, HttpContext httpContext) =>
{
    if (string.IsNullOrWhiteSpace(key) || !cache.TryGetValue<SessionBootstrapData>(key, out var data) || data == null)
        return Results.Redirect("/login");

    cache.Remove(key);

    var sessionId = Guid.NewGuid().ToString("N");
    data.SessionId = sessionId;
    cache.Set(TokenStorage.GetSessionCacheKey(sessionId), data, TimeSpan.FromHours(8));

    var claims = new List<Claim>
    {
        new(ClaimTypes.Name, data.UserName),
        new(ClaimTypes.Email, data.Email),
        new(TokenStorage.SessionClaimType, sessionId),
        new(TokenStorage.ApiTokenClaimType, data.AccessToken)
    };
    foreach (var role in data.Roles)
        claims.Add(new Claim(ClaimTypes.Role, role));
    foreach (var permission in data.Permissions)
        claims.Add(new Claim(PermissionService.ClaimType, permission));

    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    await httpContext.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        new ClaimsPrincipal(identity),
        new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) });

    httpContext.RequestServices.GetRequiredService<ApiSessionCookieService>().Write(data);

    return Results.Redirect("/");
}).DisableAntiforgery();

app.MapGet("/auth/logout", async (HttpContext httpContext, IMemoryCache cache) =>
{
    var sessionId = httpContext.User.FindFirst(TokenStorage.SessionClaimType)?.Value;
    if (!string.IsNullOrWhiteSpace(sessionId))
        cache.Remove(TokenStorage.GetSessionCacheKey(sessionId));

    httpContext.RequestServices.GetRequiredService<ApiSessionCookieService>().Delete();

    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

    var cookieOptions = new CookieOptions
    {
        Path = "/",
        SameSite = SameSiteMode.Lax,
        Secure = httpContext.Request.IsHttps,
        HttpOnly = true,
        Expires = DateTimeOffset.UnixEpoch
    };
    httpContext.Response.Cookies.Delete(CookieAuthenticationDefaults.CookiePrefix + CookieAuthenticationDefaults.AuthenticationScheme, cookieOptions);

    return Results.Redirect("/login");
}).AllowAnonymous().DisableAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
