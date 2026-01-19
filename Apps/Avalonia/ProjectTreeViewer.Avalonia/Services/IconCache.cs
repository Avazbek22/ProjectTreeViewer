using Avalonia.Media;
using Avalonia.Media.Imaging;
using ProjectTreeViewer.Kernel.Abstractions;

namespace ProjectTreeViewer.Avalonia.Services;

public sealed class IconCache
{
    private readonly IIconStore _iconStore;
    private readonly Dictionary<string, IImage> _cache = new(StringComparer.OrdinalIgnoreCase);

    public IconCache(IIconStore iconStore)
    {
        _iconStore = iconStore;
    }

    public IImage? GetIcon(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return null;

        if (_cache.TryGetValue(key, out var cached))
            return cached;

        var bytes = _iconStore.GetIconBytes(key);
        using var stream = new MemoryStream(bytes);
        var bitmap = new Bitmap(stream);
        _cache[key] = bitmap;
        return bitmap;
    }
}
