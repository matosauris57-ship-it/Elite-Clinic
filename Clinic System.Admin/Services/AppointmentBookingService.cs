using System.Net.Http.Json;
using System.Text.Json;
using DentalCare.Admin.Models;
using Microsoft.Extensions.Options;

namespace DentalCare.Admin.Services;

public class AppointmentBookingService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly AuthenticatedApiClient _apiClient;
    private readonly TokenStorage _tokenStorage;
    private readonly ApiSettings _apiSettings;

    public AppointmentBookingService(
        AuthenticatedApiClient apiClient,
        TokenStorage tokenStorage,
        IOptions<ApiSettings> apiSettings)
    {
        _apiClient = apiClient;
        _tokenStorage = tokenStorage;
        _apiSettings = apiSettings.Value;
    }

    private HttpClient Client => _apiClient.Client;

    public async Task<(List<PatientListItem> Patients, string? Error)> GetPatientsAsync()
    {
        try
        {
            using var response = await Client.GetAsync("/api/patients?includeInactive=false");

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return ([], ApiConnectionMessages.UnauthorizedSession(_tokenStorage));

            if (ApiConnectionMessages.IsRateLimited(response))
                return ([], await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<PatientListItem>>>(JsonOptions);

            if (body?.Succeeded == true && body.Data != null)
                return (body.Data, null);

            return ([], body?.Message ?? "No se pudieron cargar los pacientes.");
        }
        catch (Exception ex)
        {
            return ([], FormatConnectionError(ex) ?? $"Error de conexión: {ex.Message}");
        }
    }

    public async Task<List<DoctorListItem>> GetDoctorsAsync()
    {
        try
        {
            var response = await Client.GetFromJsonAsync<ApiResponse<List<DoctorListItem>>>("/api/doctors", JsonOptions);
            return response?.Data ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<TimeSpan>> GetAvailableSlotsAsync(int doctorId, DateTime date)
    {
        try
        {
            var url = $"/api/appointments/AvailableSlots?DoctorId={doctorId}&Date={date:yyyy-MM-dd}";
            var response = await Client.GetFromJsonAsync<ApiResponse<List<AvailableSlot>>>(url, JsonOptions);
            return response?.Data?.Select(s => s.SlotTime).ToList() ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(bool Success, string? Error, BookAppointmentResult? Result, System.Net.HttpStatusCode? StatusCode)> BookAsync(
        BookAppointmentRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await Client.PostAsJsonAsync(
                "/api/appointments/book",
                request,
                JsonOptions,
                cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return (false, ApiConnectionMessages.UnauthorizedSession(_tokenStorage), null, response.StatusCode);

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                return (false, "Su usuario no tiene el permiso agendar-cita.create.", null, response.StatusCode);

            if (ApiConnectionMessages.IsRateLimited(response))
                return (false, await ApiConnectionMessages.GetRateLimitMessageAsync(response), null, response.StatusCode);

            ApiResponse<BookAppointmentResult>? body = null;
            try
            {
                body = await response.Content.ReadFromJsonAsync<ApiResponse<BookAppointmentResult>>(
                    JsonOptions,
                    cancellationToken);
            }
            catch (JsonException)
            {
                var raw = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                    return (false, "La API devolvió una respuesta no válida al intentar agendar.", null, response.StatusCode);

                return (false, string.IsNullOrWhiteSpace(raw) ? "Respuesta inválida al agendar la cita." : raw, null, response.StatusCode);
            }

            if (body?.Succeeded == true && body.Data != null)
            {
                if (body.Data.Id <= 0)
                    return (false, "La cita no se guardó correctamente (ID inválido).", null, response.StatusCode);

                return (true, null, body.Data, response.StatusCode);
            }

            var error = FormatApiErrors(body) ?? body?.Message ?? "No se pudo agendar la cita.";
            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                error = "El horario seleccionado acaba de ser ocupado. Seleccione otro.";

            return (false, error, null, response.StatusCode);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return (false, "La API tardó demasiado en responder. Compruebe que esté ejecutándose e inténtelo nuevamente.", null, null);
        }
        catch (Exception ex)
        {
            return (false, FormatConnectionError(ex) ?? $"No se pudo agendar la cita: {ex.Message}", null, null);
        }
    }

    public async Task<(bool Success, string? Error, PatientListItem? Patient)> CreatePatientAsync(CreatePatientRequest request)
    {
        try
        {
            var response = await Client.PostAsJsonAsync("/api/patients/create", request, JsonOptions);
            if (ApiConnectionMessages.IsRateLimited(response))
                return (false, await ApiConnectionMessages.GetRateLimitMessageAsync(response), null);

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<CreatedPatientResponse>>(JsonOptions);

            if (body?.Succeeded == true && body.Data != null)
                return (true, null, MapCreatedPatient(body.Data));

            return (false, FormatApiErrors(body), null);
        }
        catch (Exception ex)
        {
            return (false, FormatConnectionError(ex) ?? ex.Message, null);
        }
    }

    private string? FormatConnectionError(Exception ex) =>
        ApiConnectionMessages.IsConnectionFailure(ex)
            ? ApiConnectionMessages.ApiUnavailable(_apiSettings.ApiBaseUrl)
            : null;

    private static PatientListItem MapCreatedPatient(CreatedPatientResponse created) => new()
    {
        Id = created.Id,
        FullName = created.FullName,
        Gender = created.Gender,
        DateOfBirth = created.DateOfBirth,
        Address = created.Address,
        Phone = created.Phone,
        NationalId = created.NationalId,
        Email = created.Email,
        CreatedAt = created.CreatedAt
    };

    private static string FormatApiErrors<T>(ApiResponse<T>? body)
    {
        if (body?.Errors?.Count > 0)
            return string.Join(" · ", body.Errors.Select(TranslateError));

        return body?.Message ?? "No se pudo completar la operación.";
    }

    private static string TranslateError(string error) => error switch
    {
        var e when e.Contains("Username must start") =>
            "El usuario debe empezar con letra e incluir al menos un número.",
        var e when e.Contains("Email is already") => "Este correo ya está registrado.",
        var e when e.Contains("Username is already") => "Este nombre de usuario ya existe.",
        var e when e.Contains("Phone number is already") => "Este teléfono ya está registrado.",
        var e when e.Contains("National ID is already") => "Esta cédula ya está registrada.",
        var e when e.Contains("Phone number must contain") =>
            "El teléfono debe tener 10–15 dígitos (solo números).",
        var e when e.Contains("Invalid email") => "Correo electrónico inválido.",
        var e when e.Contains("Date of Birth") => "La fecha de nacimiento no es válida.",
        var e when e.Contains("Password must") => "La contraseña debe tener al menos 6 caracteres.",
        var e when e.Contains("fecha y hora", StringComparison.OrdinalIgnoreCase) =>
            "La fecha y hora de la cita deben ser posteriores a la hora actual.",
        var e when e.Contains("Appointment date", StringComparison.OrdinalIgnoreCase) =>
            "La fecha de la cita no puede estar en el pasado.",
        var e when e.Contains("appointment time", StringComparison.OrdinalIgnoreCase) =>
            "Ese horario no está dentro del horario de trabajo de la clínica.",
        var e when e.Contains("horario de trabajo", StringComparison.OrdinalIgnoreCase) =>
            "Ese horario no está dentro del horario de trabajo de la clínica.",
        var e when e.Contains("Doctor not found", StringComparison.OrdinalIgnoreCase) =>
            "El médico seleccionado ya no está disponible.",
        var e when e.Contains("Patient not found", StringComparison.OrdinalIgnoreCase) =>
            "El paciente seleccionado ya no está disponible.",
        _ => error.Contains(':') ? error[(error.IndexOf(':') + 1)..].Trim() : error
    };

    public async Task<(ClinicSchedule? Schedule, string? Error)> GetClinicScheduleAsync()
    {
        try
        {
            using var response = await Client.GetAsync("/api/clinic/schedule");
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return (null, ApiConnectionMessages.UnauthorizedSession(_tokenStorage));
            if (ApiConnectionMessages.IsRateLimited(response))
                return (null, await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<ClinicSchedule>>(JsonOptions);
            if (body?.Succeeded == true && body.Data != null)
                return (body.Data, null);

            return (null, body?.Message ?? "No se pudo cargar el horario de la clínica.");
        }
        catch (Exception ex)
        {
            return (null, FormatConnectionError(ex) ?? $"Error de conexión: {ex.Message}");
        }
    }

    public async Task<(bool Success, string? Error, ClinicSchedule? Schedule)> SaveClinicScheduleAsync(ClinicSchedule schedule)
    {
        try
        {
            using var response = await Client.PutAsJsonAsync("/api/clinic/schedule", schedule, JsonOptions);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return (false, ApiConnectionMessages.UnauthorizedSession(_tokenStorage), null);
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                return (false, "No tiene permiso para cambiar el horario de la clínica.", null);
            if (ApiConnectionMessages.IsRateLimited(response))
                return (false, await ApiConnectionMessages.GetRateLimitMessageAsync(response), null);

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<ClinicSchedule>>(JsonOptions);
            if (body?.Succeeded == true)
                return (true, null, body.Data);

            return (false, FormatApiErrors(body) ?? "No se pudo guardar el horario.", null);
        }
        catch (Exception ex)
        {
            return (false, FormatConnectionError(ex) ?? $"Error de conexión: {ex.Message}", null);
        }
    }

    public async Task<(bool Success, string? Error)> SaveClinicEmailSettingsAsync(ClinicEmailSettingsRequest settings)
    {
        try
        {
            using var response = await Client.PutAsJsonAsync("/api/clinic/email-settings", settings, JsonOptions);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return (false, ApiConnectionMessages.UnauthorizedSession(_tokenStorage));
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                return (false, "No tiene permiso para cambiar el correo de la clínica.");
            if (ApiConnectionMessages.IsRateLimited(response))
                return (false, await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<ClinicEmailSettingsRequest>>(JsonOptions);
            if (body?.Succeeded == true)
                return (true, null);

            return (false, FormatApiErrors(body) ?? "No se pudo guardar el SMTP.");
        }
        catch (Exception ex)
        {
            return (false, FormatConnectionError(ex) ?? $"Error de conexión: {ex.Message}");
        }
    }

    public async Task<(bool Success, string? Error)> SendPatientEmailAsync(string to, string subject, string body)
    {
        try
        {
            using var response = await Client.PostAsJsonAsync("/api/clinic/send-email", new
            {
                To = to,
                Subject = subject,
                Body = body
            }, JsonOptions);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return (false, ApiConnectionMessages.UnauthorizedSession(_tokenStorage));
            if (ApiConnectionMessages.IsRateLimited(response))
                return (false, await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var parsed = await response.Content.ReadFromJsonAsync<ApiResponse<string>>(JsonOptions);
            if (parsed?.Succeeded == true)
                return (true, null);

            return (false, FormatApiErrors(parsed) ?? "No se pudo enviar el correo.");
        }
        catch (Exception ex)
        {
            return (false, FormatConnectionError(ex) ?? $"Error de conexión: {ex.Message}");
        }
    }

    public async Task<(PatientNotificationApiModel? Settings, string? Error)> GetPatientNotificationsAsync()
    {
        try
        {
            using var response = await Client.GetAsync("/api/clinic/patient-notifications");
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return (null, ApiConnectionMessages.UnauthorizedSession(_tokenStorage));
            if (ApiConnectionMessages.IsRateLimited(response))
                return (null, await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<PatientNotificationApiModel>>(JsonOptions);
            if (body?.Succeeded == true && body.Data != null)
                return (body.Data, null);

            return (null, body?.Message ?? "No se pudieron cargar los avisos automáticos.");
        }
        catch (Exception ex)
        {
            return (null, FormatConnectionError(ex) ?? $"Error de conexión: {ex.Message}");
        }
    }

    public async Task<(bool Success, string? Error)> SavePatientNotificationsAsync(PatientNotificationApiModel settings)
    {
        try
        {
            using var response = await Client.PutAsJsonAsync("/api/clinic/patient-notifications", settings, JsonOptions);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return (false, ApiConnectionMessages.UnauthorizedSession(_tokenStorage));
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                return (false, "No tiene permiso para cambiar los avisos automáticos.");
            if (ApiConnectionMessages.IsRateLimited(response))
                return (false, await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<PatientNotificationApiModel>>(JsonOptions);
            if (body?.Succeeded == true)
                return (true, null);

            return (false, FormatApiErrors(body) ?? "No se pudieron guardar los avisos automáticos.");
        }
        catch (Exception ex)
        {
            return (false, FormatConnectionError(ex) ?? $"Error de conexión: {ex.Message}");
        }
    }

    public static List<TimeSpan> GenerateAllSlots(TimeSpan start, TimeSpan end, int durationMinutes)
    {
        var slots = new List<TimeSpan>();
        var current = start;
        var minutes = durationMinutes is < 5 or > 120 ? 15 : durationMinutes;
        var step = TimeSpan.FromMinutes(minutes);
        while (current.Add(step) <= end)
        {
            slots.Add(current);
            current = current.Add(step);
        }
        return slots;
    }
}
