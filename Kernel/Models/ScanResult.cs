namespace ProjectTreeViewer.Kernel.Models;

public sealed record ScanResult<T>(
	T Value,
	bool RootAccessDenied,
	bool HadAccessDenied);
