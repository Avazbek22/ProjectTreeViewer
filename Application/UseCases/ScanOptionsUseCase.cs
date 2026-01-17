using ProjectTreeViewer.Kernel.Abstractions;
using ProjectTreeViewer.Kernel.Contracts;

namespace ProjectTreeViewer.Application.UseCases;

public sealed class ScanOptionsUseCase
{
	private readonly IFileSystemScanner _scanner;

	public ScanOptionsUseCase(IFileSystemScanner scanner)
	{
		_scanner = scanner;
	}

	public ScanOptionsResult Execute(ScanOptionsRequest request)
	{
		var extensions = _scanner.GetExtensions(request.RootPath, request.IgnoreRules, request.AllowedRootFolders);
		var rootFolders = _scanner.GetRootFolderNames(request.RootPath, request.IgnoreRules);

		return new ScanOptionsResult(
			Extensions: extensions.Value.OrderBy(v => v, StringComparer.OrdinalIgnoreCase).ToList(),
			RootFolders: rootFolders.Value.OrderBy(v => v, StringComparer.OrdinalIgnoreCase).ToList(),
			RootAccessDenied: extensions.RootAccessDenied || rootFolders.RootAccessDenied,
			HadAccessDenied: extensions.HadAccessDenied || rootFolders.HadAccessDenied);
	}

	public bool CanReadRoot(string rootPath) => _scanner.CanReadRoot(rootPath);
}
