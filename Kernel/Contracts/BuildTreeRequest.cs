using ProjectTreeViewer.Kernel.Models;

namespace ProjectTreeViewer.Kernel.Contracts;

public sealed record BuildTreeRequest(
	string RootPath,
	TreeFilterOptions Filter);
