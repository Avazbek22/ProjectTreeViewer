using DevProjex.Kernel.Models;

namespace DevProjex.Kernel.Abstractions;

public interface IIconMapper
{
	string GetIconKey(FileSystemNode node);
}
