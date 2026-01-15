using System;
using System.IO;
using System.Text;
using ProjectTreeViewer.Kernel.Abstractions;

namespace ProjectTreeViewer.Infrastructure.Resources;

public sealed class ResourceTextReader
{
    private readonly IResourceStore _store;

    public ResourceTextReader(IResourceStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public bool TryReadAllText(string resourceId, out string text)
    {
        text = string.Empty;

        if (!_store.TryOpenRead(resourceId, out var stream))
            return false;

        try
        {
            using (stream)
            using (var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
            {
                text = reader.ReadToEnd();
                return true;
            }
        }
        catch
        {
            text = string.Empty;
            return false;
        }
    }
}