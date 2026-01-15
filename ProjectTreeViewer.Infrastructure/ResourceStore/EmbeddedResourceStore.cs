using System;
using System.IO;
using System.Linq;
using System.Reflection;
using ProjectTreeViewer.Kernel.Abstractions;

namespace ProjectTreeViewer.Infrastructure.ResourceStore;

public sealed class EmbeddedResourceStore : IResourceStore
{
    private readonly Assembly _assembly;
    private readonly string _baseNamespace;
    private readonly string[] _resourceNames;

    public EmbeddedResourceStore(Assembly assembly, string? baseNamespace = null)
    {
        _assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
        _baseNamespace = string.IsNullOrWhiteSpace(baseNamespace) ? assembly.GetName().Name! : baseNamespace!;
        _resourceNames = _assembly.GetManifestResourceNames();
    }

    public bool Exists(string resourceId)
    {
        var manifestName = ToManifestName(resourceId);
        return _resourceNames.Contains(manifestName, StringComparer.Ordinal);
    }

    public bool TryOpenRead(string resourceId, out Stream stream)
    {
        stream = Stream.Null;

        var manifestName = ToManifestName(resourceId);
        var s = _assembly.GetManifestResourceStream(manifestName);
        if (s is null)
            return false;

        stream = s;
        return true;
    }

    private string ToManifestName(string resourceId)
    {
        // Пример resourceId:
        // "Localization/ru.json"
        // "IconPacks/Default/manifest.json"
        // "IconPacks/Default/icons/folder24.png"

        var normalized = (resourceId ?? string.Empty)
            .Replace('\\', '/')
            .TrimStart('/')
            .Replace('/', '.');

        return $"{_baseNamespace}.{normalized}";
    }
}