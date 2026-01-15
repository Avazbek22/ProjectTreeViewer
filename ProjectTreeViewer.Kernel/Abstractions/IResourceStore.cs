using System.IO;

namespace ProjectTreeViewer.Kernel.Abstractions;

public interface IResourceStore
{
    bool Exists(string resourceId);
    bool TryOpenRead(string resourceId, out Stream stream);
}