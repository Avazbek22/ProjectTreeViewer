using ProjectTreeViewer.Infrastructure.Icons;
using ProjectTreeViewer.Kernel.Abstractions;

namespace ProjectTreeViewer;

public static class IconPackProvider
{
    public static IIconPackService CreateDefault()
    {
        var store = AssetsResourceStoreFactory.CreateEmbeddedAssetsStore();
        return new IconPackService(store, packId: "Default");
    }
}