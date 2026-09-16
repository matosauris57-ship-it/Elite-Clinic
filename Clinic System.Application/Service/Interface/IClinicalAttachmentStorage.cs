namespace Clinic_System.Application.Service.Interface;

public interface IClinicalAttachmentStorage
{
    long MaxFileBytes { get; }

    bool IsAllowed(string fileName, string? contentType);

    Task<StoredClinicalAttachment> SaveAsync(
        int patientId,
        string originalFileName,
        string? contentType,
        Stream content,
        CancellationToken cancellationToken = default);

    Stream OpenRead(int patientId, string storedFileName);

    void Delete(int patientId, string storedFileName);
}

public sealed record StoredClinicalAttachment(
    string StoredFileName,
    string ContentType,
    long FileSizeBytes,
    string OriginalFileName);
