using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Clinic_System.Core.Enums;
using DentalCare.Admin.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Options;

namespace DentalCare.Admin.Services;

public class PatientClinicalAttachmentService
{
    public const long MaxFileBytes = 20 * 1024 * 1024;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly AuthenticatedApiClient _apiClient;
    private readonly TokenStorage _tokenStorage;
    private readonly ApiSettings _apiSettings;

    public PatientClinicalAttachmentService(
        AuthenticatedApiClient apiClient,
        TokenStorage tokenStorage,
        IOptions<ApiSettings> apiSettings)
    {
        _apiClient = apiClient;
        _tokenStorage = tokenStorage;
        _apiSettings = apiSettings.Value;
    }

    private HttpClient Client => _apiClient.Client;

    public async Task<(List<PatientClinicalAttachmentItem> Items, string? Error)> ListAsync(int patientId)
    {
        try
        {
            using var response = await Client.GetAsync($"/api/patients/{patientId}/clinical-attachments");
            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return ([], ApiConnectionMessages.UnauthorizedSession(_tokenStorage));
            if (ApiConnectionMessages.IsRateLimited(response))
                return ([], await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<PatientClinicalAttachmentItem>>>(JsonOptions);
            return body?.Succeeded == true && body.Data != null
                ? (body.Data, null)
                : ([], body?.Message ?? "No se pudieron cargar los adjuntos clínicos.");
        }
        catch (Exception ex)
        {
            return ([], FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(PatientClinicalAttachmentItem? Item, string? Error)> UploadAsync(
        int patientId,
        IBrowserFile file,
        ClinicalAttachmentKind kind,
        string? title,
        string? subtype,
        string? notes,
        int? toothNumber,
        DateTime? capturedOn,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var content = new MultipartFormDataContent();
            await using var stream = file.OpenReadStream(MaxFileBytes, cancellationToken);
            var streamContent = new StreamContent(stream);
            if (!string.IsNullOrWhiteSpace(file.ContentType))
                streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse(file.ContentType);
            content.Add(streamContent, "File", file.Name);
            content.Add(new StringContent(((int)kind).ToString()), "Kind");
            if (!string.IsNullOrWhiteSpace(title))
                content.Add(new StringContent(title), "Title");
            if (!string.IsNullOrWhiteSpace(subtype))
                content.Add(new StringContent(subtype), "Subtype");
            if (!string.IsNullOrWhiteSpace(notes))
                content.Add(new StringContent(notes), "Notes");
            if (toothNumber is > 0)
                content.Add(new StringContent(toothNumber.Value.ToString()), "ToothNumber");
            if (capturedOn.HasValue)
                content.Add(new StringContent(capturedOn.Value.ToString("yyyy-MM-dd")), "CapturedOn");

            using var response = await Client.PostAsync($"/api/patients/{patientId}/clinical-attachments", content, cancellationToken);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return (null, ApiConnectionMessages.UnauthorizedSession(_tokenStorage));
            if (ApiConnectionMessages.IsRateLimited(response))
                return (null, await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<PatientClinicalAttachmentItem>>(JsonOptions, cancellationToken);
            return body?.Succeeded == true && body.Data != null
                ? (body.Data, null)
                : (null, body?.Message ?? "No se pudo guardar el adjunto.");
        }
        catch (Exception ex)
        {
            return (null, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(int patientId, int attachmentId, UpdateClinicalAttachmentForm form)
    {
        try
        {
            using var response = await Client.PutAsJsonAsync(
                $"/api/patients/{patientId}/clinical-attachments/{attachmentId}",
                form,
                JsonOptions);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return (false, ApiConnectionMessages.UnauthorizedSession(_tokenStorage));
            if (ApiConnectionMessages.IsRateLimited(response))
                return (false, await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<PatientClinicalAttachmentItem>>(JsonOptions);
            return body?.Succeeded == true
                ? (true, null)
                : (false, body?.Message ?? "No se pudo actualizar el adjunto.");
        }
        catch (Exception ex)
        {
            return (false, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int patientId, int attachmentId)
    {
        try
        {
            using var response = await Client.DeleteAsync($"/api/patients/{patientId}/clinical-attachments/{attachmentId}");
            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return (false, ApiConnectionMessages.UnauthorizedSession(_tokenStorage));
            if (ApiConnectionMessages.IsRateLimited(response))
                return (false, await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);
            return body?.Succeeded == true
                ? (true, null)
                : (false, body?.Message ?? "No se pudo eliminar el adjunto.");
        }
        catch (Exception ex)
        {
            return (false, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(Stream? Stream, string ContentType, string FileName, string? Error)> OpenFileAsync(
        int patientId,
        int attachmentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await Client.GetAsync(
                $"/api/patients/{patientId}/clinical-attachments/{attachmentId}/file",
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                response.Dispose();
                return (null, "", "", ApiConnectionMessages.UnauthorizedSession(_tokenStorage));
            }

            if (ApiConnectionMessages.IsRateLimited(response))
            {
                var message = await ApiConnectionMessages.GetRateLimitMessageAsync(response);
                response.Dispose();
                return (null, "", "", message);
            }

            if (!response.IsSuccessStatusCode)
            {
                response.Dispose();
                return (null, "", "", "No se pudo abrir el archivo.");
            }

            var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
            var fileName = response.Content.Headers.ContentDisposition?.FileNameStar
                ?? response.Content.Headers.ContentDisposition?.FileName?.Trim('"')
                ?? "archivo";
            var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            return (new HttpResponseStream(response, stream), contentType, fileName, null);
        }
        catch (Exception ex)
        {
            return (null, "", "", FormatConnectionError(ex) ?? ex.Message);
        }
    }

    private string? FormatConnectionError(Exception ex) =>
        ApiConnectionMessages.IsConnectionFailure(ex)
            ? ApiConnectionMessages.ApiUnavailable(_apiSettings.ApiBaseUrl)
            : null;

    private sealed class HttpResponseStream : Stream
    {
        private readonly HttpResponseMessage _response;
        private readonly Stream _inner;

        public HttpResponseStream(HttpResponseMessage response, Stream inner)
        {
            _response = response;
            _inner = inner;
        }

        public override bool CanRead => _inner.CanRead;
        public override bool CanSeek => _inner.CanSeek;
        public override bool CanWrite => false;
        public override long Length => _inner.CanSeek ? _inner.Length : 0;
        public override long Position { get => _inner.Position; set => _inner.Position = value; }
        public override void Flush() => _inner.Flush();
        public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
        public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
            _inner.ReadAsync(buffer, cancellationToken);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _inner.Dispose();
                _response.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
