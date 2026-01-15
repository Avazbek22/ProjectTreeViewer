using ProjectTreeViewer.Assets;
using ProjectTreeViewer.Infrastructure.ResourceStore;
using ProjectTreeViewer.Kernel.Abstractions;

namespace ProjectTreeViewer;

public static class AssetsResourceStoreFactory
{
    public static IResourceStore CreateEmbeddedAssetsStore()
    {
        return new EmbeddedResourceStore(
            AssetsAssembly.Assembly,
            baseNamespace: "ProjectTreeViewer.Assets");
    }
}