using System.Text;
using Clinic_System.Application.Service.Interface;

namespace Clinic_System.API.Services;

public sealed class FileClinicalAttachmentStorage : IClinicalAttachmentStorage
{
    public const long DefaultMaxFileBytes = 20 * 1024 * 1024;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".gif", ".pdf", ".doc", ".docx"
    };

    private static readonly Dictionary<string, string> ContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".png"] = "image/png",
        [".webp"] = "image/webp",
        [".gif"] = "image/gif",
        [".pdf"] = "application/pdf",
        [".doc"] = "application/msword",
        [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    };

    private readonly string _root;

    public FileClinicalAttachmentStorage(IWebHostEnvironment environment)
    {
        _root = Path.Combine(environment.ContentRootPath, "App_Data", "patient-attachments");
    }

    public long MaxFileBytes => DefaultMaxFileBytes;

    public bool IsAllowed(string fileName, string? contentType)
    {
        var extension = Path.GetExtension(fileName);
        return !string.IsNullOrWhiteSpace(extension) && AllowedExtensions.Contains(extension);
    }

    public async Task<StoredClinicalAttachment> SaveAsync(
        int patientId,
        string originalFileName,
        string? contentType,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        var safeOriginal = SanitizeOriginalName(originalFileName);
        var extension = Path.GetExtension(safeOriginal).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            throw new InvalidOperationException("Formato no permitido. Use JPG, PNG, WEBP, GIF, PDF o Word.");

        var storedName = $"{Guid.NewGuid():N}{extension}";
        var directory = PatientDirectory(patientId);
        Directory.CreateDirectory(directory);
        var fullPath = Path.Combine(directory, storedName);

        await using (var output = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 64 * 1024, useAsync: true))
        {
            var buffer = new byte[64 * 1024];
            long total = 0;
            int read;
            while ((read = await content.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)) > 0)
            {
                total += read;
                if (total > MaxFileBytes)
                {
                    await output.DisposeAsync();
                    TryDelete(fullPath);
                    throw new InvalidOperationException("El archivo no puede superar 20 MB.");
                }

                await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            }

            if (total <= 0)
            {
                TryDelete(fullPath);
                throw new InvalidOperationException("El archivo está vacío.");
            }

            var resolvedType = ContentTypes.GetValueOrDefault(extension) ?? "application/octet-stream";
            return new StoredClinicalAttachment(storedName, resolvedType, total, safeOriginal);
        }
    }

    public Stream OpenRead(int patientId, string storedFileName)
    {
        var path = ResolveExistingPath(patientId, storedFileName);
        return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 64 * 1024, useAsync: true);
    }

    public void Delete(int patientId, string storedFileName)
    {
        try
        {
            var path = ResolveExistingPath(patientId, storedFileName);
            TryDelete(path);
        }
        catch (FileNotFoundException)
        {
        }
    }

    private string PatientDirectory(int patientId) => Path.Combine(_root, patientId.ToString());

    private string ResolveExistingPath(int patientId, string storedFileName)
    {
        var name = Path.GetFileName(storedFileName);
        if (string.IsNullOrWhiteSpace(name) || name != storedFileName)
            throw new FileNotFoundException("Archivo clínico no encontrado.");

        var fullRoot = Path.GetFullPath(_root);
        var fullPath = Path.GetFullPath(Path.Combine(PatientDirectory(patientId), name));
        if (!fullPath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase) || !File.Exists(fullPath))
            throw new FileNotFoundException("Archivo clínico no encontrado.");

        return fullPath;
    }

    private static string SanitizeOriginalName(string? fileName)
    {
        var name = Path.GetFileName(fileName ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name))
            name = "archivo";

        var builder = new StringBuilder(name.Length);
        foreach (var ch in name)
        {
            if (ch is '\\' or '/' or ':' or '*' or '?' or '"' or '<' or '>' or '|')
                builder.Append('_');
            else
                builder.Append(ch);
        }

        var sanitized = builder.ToString();
        return sanitized.Length > 200 ? sanitized[..200] : sanitized;
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch
        {
            // ignore leftover file; metadata is the source of truth
        }
    }
}
