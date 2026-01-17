using System.Collections.Generic;
using ProjectTreeViewer.Kernel.Models;

namespace ProjectTreeViewer.Application.Services;

public sealed class IgnoreRulesService
{
	public IgnoreRules Build(
		IReadOnlyCollection<IgnoreOptionDescriptor> options,
		IReadOnlyCollection<string> selectedOptions)
	{
		var selected = new HashSet<string>(selectedOptions, StringComparer.OrdinalIgnoreCase);
		var folders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		var files = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		bool ignoreHiddenFolders = false;
		bool ignoreHiddenFiles = false;
		bool ignoreDotFolders = false;
		bool ignoreDotFiles = false;

		foreach (var option in options)
		{
			if (!selected.Contains(option.Id))
				continue;

			switch (option.Kind)
			{
				case IgnoreOptionKind.NamedFolder:
					folders.Add(option.Id);
					break;
				case IgnoreOptionKind.NamedFile:
					files.Add(option.Id);
					break;
				case IgnoreOptionKind.HiddenFolders:
					ignoreHiddenFolders = true;
					break;
				case IgnoreOptionKind.HiddenFiles:
					ignoreHiddenFiles = true;
					break;
				case IgnoreOptionKind.DotFolders:
					ignoreDotFolders = true;
					break;
				case IgnoreOptionKind.DotFiles:
					ignoreDotFiles = true;
					break;
			}
		}

		return new IgnoreRules(
			IgnoreHiddenFolders: ignoreHiddenFolders,
			IgnoreHiddenFiles: ignoreHiddenFiles,
			IgnoreDotFolders: ignoreDotFolders,
			IgnoreDotFiles: ignoreDotFiles,
			SmartIgnoredFolders: folders,
			SmartIgnoredFiles: files);
	}
}
