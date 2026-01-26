using Avalonia.Media;
using Avalonia.Media.Imaging;
using DevProjex.Kernel.Abstractions;

namespace DevProjex.Avalonia.Services;

public sealed class IconCache
{
    /// <summary>
    /// Maximum number of cached icons. Icons are typically limited (~50-100 types),
    /// but this provides a safety bound to prevent unbounded growth.
    /// </summary>
    private const int MaxCacheSize = 256;

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

        // Safety check: clear cache if it grows too large (shouldn't happen in practice)
        if (_cache.Count >= MaxCacheSize)
            _cache.Clear();

        var bytes = _iconStore.GetIconBytes(key);
        using var stream = new MemoryStream(bytes);
        var bitmap = new Bitmap(stream);
        _cache[key] = bitmap;
        return bitmap;
    }
}
