using ProjectTreeViewer.Kernel.Models;

namespace ProjectTreeViewer.Kernel.Abstractions;

public interface ITreeBuilder
{
	TreeBuildResult Build(string rootPath, TreeFilterOptions options);
}
