using System;
using System.Text.Json;
using ProjectTreeViewer.Kernel.Abstractions;

namespace ProjectTreeViewer.Infrastructure.Resources;

public sealed class ResourceJsonReader
{
    private readonly ResourceTextReader _textReader;

    public ResourceJsonReader(IResourceStore store)
    {
        _textReader = new ResourceTextReader(store);
    }

    public bool TryReadJson<T>(string resourceId, out T? value)
    {
        value = default;

        if (!_textReader.TryReadAllText(resourceId, out var json))
            return false;

        try
        {
            value = JsonSerializer.Deserialize<T>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                });

            return value is not null;
        }
        catch
        {
            value = default;
            return false;
        }
    }
}