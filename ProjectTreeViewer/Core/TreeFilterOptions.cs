using System.Collections.Generic;

namespace ProjectTreeViewer;

public sealed record TreeFilterOptions(
	IReadOnlySet<string> AllowedExtensions,
	IReadOnlySet<string> AllowedRootFolders,
	bool IgnoreBin,
	bool IgnoreObj,
	bool IgnoreDot);
