using ProjectTreeViewer.Infrastructure.Icons;
using ProjectTreeViewer.Kernel.Abstractions;

namespace ProjectTreeViewer;

public static class IconPackProvider
{
    public static IIconPackService CreateDefault()
    {
        var store = AssetsResourceStoreFactory.CreateEmbeddedAssetsStore();
        return CreateDefault(store);
    }

    public static IIconPackService CreateDefault(IResourceStore store)
    {
        return new IconPackService(store, defaultPackId: "Default");
    }
}