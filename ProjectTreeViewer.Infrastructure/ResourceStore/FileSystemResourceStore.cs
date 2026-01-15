using System;
using System.IO;
using ProjectTreeViewer.Kernel.Abstractions;

namespace ProjectTreeViewer.Infrastructure.ResourceStore;

public sealed class FileSystemResourceStore : IResourceStore
{
    private readonly string _rootDirectory;

    public FileSystemResourceStore(string rootDirectory)
    {
        if (string.IsNullOrWhiteSpace(rootDirectory))
            throw new ArgumentException("Root directory is empty.", nameof(rootDirectory));

        _rootDirectory = Path.GetFullPath(rootDirectory);
    }

    public bool Exists(string resourceId)
    {
        var fullPath = MapToFullPath(resourceId);
        return File.Exists(fullPath);
    }

    public bool TryOpenRead(string resourceId, out Stream stream)
    {
        stream = Stream.Null;

        var fullPath = MapToFullPath(resourceId);
        if (!File.Exists(fullPath))
            return false;

        try
        {
            stream = File.Open(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return true;
        }
        catch
        {
            stream = Stream.Null;
            return false;
        }
    }

    private string MapToFullPath(string resourceId)
    {
        var rel = (resourceId ?? string.Empty)
            .Replace('\\', '/')
            .TrimStart('/')
            .Replace('/', Path.DirectorySeparatorChar);

        var full = Path.GetFullPath(Path.Combine(_rootDirectory, rel));

        // Защита от выхода из корня (..)
        if (!full.StartsWith(_rootDirectory, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Resource id escapes root directory: '{resourceId}'.");

        return full;
    }
}