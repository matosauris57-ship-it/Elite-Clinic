namespace DentalCare.Admin.Services;

public class ApiAvailabilityHostedService : IHostedService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ApiAvailabilityHostedService> _logger;

    public ApiAvailabilityHostedService(IConfiguration configuration, ILogger<ApiAvailabilityHostedService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = CheckApiAvailabilityAsync(cancellationToken);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task CheckApiAvailabilityAsync(CancellationToken cancellationToken)
    {
        var apiUrl = _configuration["ApiSettings:ApiBaseUrl"] ?? "http://localhost:5129";

        try
        {
            using var client = new HttpClient
            {
                BaseAddress = new Uri(apiUrl),
                Timeout = TimeSpan.FromSeconds(3)
            };
            await client.GetAsync("/swagger/index.html", cancellationToken);
        }
        catch (Exception ex) when (ApiConnectionMessages.IsConnectionFailure(ex) || ex is TaskCanceledException or TimeoutException)
        {
            _logger.LogWarning(
                "La API en {ApiUrl} no está disponible. Usa el perfil \"API + Admin\" para depurar.",
                apiUrl);
        }
    }
}
