namespace DentalCare.Admin.Services;

public sealed class AuthenticatedApiClient
{
    public AuthenticatedApiClient(HttpClient httpClient)
    {
        Client = httpClient;
    }

    public HttpClient Client { get; }
}
