using System.Collections.Generic;
using ProjectTreeViewer.Kernel.Models;

namespace ProjectTreeViewer.Kernel.Abstractions;

public interface ISmartIgnoreAnalyzer
{
	IReadOnlyList<IgnoreOptionDefinition> Analyze(string rootPath, IReadOnlySet<string> allowedRootFolders);
}
