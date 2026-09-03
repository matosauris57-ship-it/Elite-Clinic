namespace DentalCare.Admin.Services;

public sealed class LoadingMediaCatalog
{
    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".gif", ".webp", ".png", ".apng"
    };

    private static readonly HashSet<string> VideoExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp4", ".webm"
    };

    private readonly string _directory;

    public LoadingMediaCatalog(IWebHostEnvironment environment)
    {
        var webRoot = environment.WebRootPath
            ?? Path.Combine(environment.ContentRootPath, "wwwroot");
        _directory = Path.Combine(webRoot, "images", "loaders");
    }

    public LoadingMediaItem? PickRandom()
    {
        var items = ListItems();
        if (items.Count == 0)
            return null;

        return items[Random.Shared.Next(items.Count)];
    }

    private IReadOnlyList<LoadingMediaItem> ListItems()
    {
        if (!Directory.Exists(_directory))
            return [];

        return Directory.EnumerateFiles(_directory)
            .Select(TryCreate)
            .OfType<LoadingMediaItem>()
            .ToArray();
    }

    private static LoadingMediaItem? TryCreate(string path)
    {
        var extension = Path.GetExtension(path);
        var fileName = Path.GetFileName(path);
        if (string.IsNullOrWhiteSpace(fileName) || fileName.StartsWith('.'))
            return null;

        var url = $"/images/loaders/{fileName}";

        if (ImageExtensions.Contains(extension))
        {
            return new LoadingMediaItem(url, IsVideo: false, MimeType(extension));
        }

        if (VideoExtensions.Contains(extension))
        {
            return new LoadingMediaItem(url, IsVideo: true, MimeType(extension));
        }

        return null;
    }

    private static string MimeType(string extension) => extension.ToLowerInvariant() switch
    {
        ".gif" => "image/gif",
        ".webp" => "image/webp",
        ".png" or ".apng" => "image/png",
        ".webm" => "video/webm",
        _ => "video/mp4"
    };
}

public sealed record LoadingMediaItem(string Url, bool IsVideo, string MimeType);
