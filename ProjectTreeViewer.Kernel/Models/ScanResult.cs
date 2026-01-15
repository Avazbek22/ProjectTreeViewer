namespace ProjectTreeViewer;

public sealed record ScanResult<T>(
    T Value,
    bool RootAccessDenied,
    bool HadAccessDenied);