using DevProjex.Kernel.Models;

namespace DevProjex.Kernel.Abstractions;

public interface IFileSystemScanner
{
	bool CanReadRoot(string rootPath);
	ScanResult<HashSet<string>> GetExtensions(string rootPath, IgnoreRules rules);
	ScanResult<HashSet<string>> GetRootFileExtensions(string rootPath, IgnoreRules rules);
	ScanResult<List<string>> GetRootFolderNames(string rootPath, IgnoreRules rules);
}
