using System.Collections.Generic;

namespace ProjectTreeViewer;

public sealed record ProjectTreeOptions(
	IReadOnlySet<string> AllowedExtensions,
	IReadOnlySet<string> AllowedRootFolders,
	bool IgnoreBin,
	bool IgnoreObj,
	bool IgnoreDot,
	bool IncludePathHeader);
