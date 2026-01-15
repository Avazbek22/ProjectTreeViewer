namespace ProjectTreeViewer.Kernel.Abstractions;

public interface IIconPackService
{
    string PackId { get; }
    IconPackManifest Manifest { get; }

    void SetPack(string packId);

    string GetIconResourceIdForPath(string fullPath, bool isDirectory);

    bool IsGrayFolderName(string folderName);

    bool IsContentExcludedFile(string fileName);
}