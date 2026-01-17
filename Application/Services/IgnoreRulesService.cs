using System.Collections.Generic;
using System.Linq;
using ProjectTreeViewer.Kernel.Models;

namespace ProjectTreeViewer.Application.Services;

public sealed class IgnoreRulesService
{
	private readonly SmartIgnoreService _smartIgnore;

	public IgnoreRulesService(SmartIgnoreService smartIgnore)
	{
		_smartIgnore = smartIgnore;
	}

	public IgnoreRules Build(string rootPath, IReadOnlyCollection<IgnoreOptionId> selectedOptions)
	{
		var smart = _smartIgnore.Build(rootPath);

		return new IgnoreRules(
			IgnoreBinFolders: selectedOptions.Contains(IgnoreOptionId.BinFolders),
			IgnoreObjFolders: selectedOptions.Contains(IgnoreOptionId.ObjFolders),
			IgnoreHiddenFolders: selectedOptions.Contains(IgnoreOptionId.HiddenFolders),
			IgnoreHiddenFiles: selectedOptions.Contains(IgnoreOptionId.HiddenFiles),
			IgnoreDotFolders: selectedOptions.Contains(IgnoreOptionId.DotFolders),
			IgnoreDotFiles: selectedOptions.Contains(IgnoreOptionId.DotFiles),
			SmartIgnoredFolders: smart.FolderNames,
			SmartIgnoredFiles: smart.FileNames);
	}
}
