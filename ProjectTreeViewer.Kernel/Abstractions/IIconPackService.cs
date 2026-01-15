namespace ProjectTreeViewer.Kernel.Abstractions;

public interface IIconPackService
{
    string PackId { get; }

    string ResolveIconKey(string itemName, bool isDirectory);

    string ResolveIconResourceId(string itemName, bool isDirectory);

    string ResolveIconResourceIdByKey(string iconKey);

    bool IsContentExcludedByName(string fileName);
}