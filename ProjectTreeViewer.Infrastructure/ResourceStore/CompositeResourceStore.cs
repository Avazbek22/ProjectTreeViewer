using System;
using System.Collections.Generic;
using System.IO;
using ProjectTreeViewer.Kernel.Abstractions;

namespace ProjectTreeViewer.Infrastructure.ResourceStore;

public sealed class CompositeResourceStore : IResourceStore
{
    private readonly IReadOnlyList<IResourceStore> _stores;

    public CompositeResourceStore(params IResourceStore[] stores)
    {
        if (stores is null || stores.Length == 0)
            throw new ArgumentException("At least one resource store is required.", nameof(stores));

        _stores = stores;
    }

    public bool Exists(string resourceId)
    {
        foreach (var s in _stores)
        {
            if (s.Exists(resourceId))
                return true;
        }

        return false;
    }

    public bool TryOpenRead(string resourceId, out Stream stream)
    {
        foreach (var s in _stores)
        {
            if (s.TryOpenRead(resourceId, out stream))
                return true;
        }

        stream = Stream.Null;
        return false;
    }
}