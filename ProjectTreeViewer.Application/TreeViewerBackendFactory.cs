using ProjectTreeViewer.Infrastructure.Localization;
using ProjectTreeViewer.Kernel.Abstractions;

namespace ProjectTreeViewer;

public static class TreeViewerBackendFactory
{
    public static TreeViewerBackend CreateDefault()
    {
        // 1) Ресурсы из ProjectTreeViewer.Assets (embedded)
        IResourceStore assetsStore = AssetsResourceStoreFactory.CreateEmbeddedAssetsStore();

        // 2) Локализация из ресурсов
        ILocalizationCatalog localizationCatalog = new ResourceLocalizationCatalog(assetsStore);

        // 3) Иконпак из ресурсов (тот же store)
        IIconPackService iconPackService = IconPackProvider.CreateDefault(assetsStore);

        // 4) “Юзкейсы” (Application)
        IFileSystemScanner scanner = new FileSystemScanner();
        ITreeBuilder builder = new TreeBuilder();
        ISelectedContentExportService selectedContent = new SelectedContentExportService();
        ITreeTextExportService treeText = new TreeTextExportService();

        return new TreeViewerBackend(
            scanner: scanner,
            builder: builder,
            contentExporter: selectedContent,
            icons: iconPackService,
            localizationCatalog: localizationCatalog,
            treeTextExporter: treeText
        );
    }
}