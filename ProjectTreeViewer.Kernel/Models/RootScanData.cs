using System.Collections.Generic;

namespace ProjectTreeViewer;

public sealed record RootScanData(
    IReadOnlyList<string> RootFolderNames,
    IReadOnlySet<string> Extensions,
    bool RootAccessDenied,
    bool HadAccessDenied);