using ProjectTreeViewer.Kernel.Models;

namespace ProjectTreeViewer.Kernel.Abstractions;

public interface IIconMapper
{
	string GetIconKey(FileSystemNode node);
}
