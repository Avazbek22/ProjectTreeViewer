using System;
using System.Collections.Generic;
using System.Linq;
using ProjectTreeViewer.Application.Models;
using ProjectTreeViewer.Kernel.Models;

namespace ProjectTreeViewer.Application.Services;

public sealed class FilterOptionSelectionService
{
	public IReadOnlyList<SelectionOption> BuildExtensionOptions(
		IEnumerable<string> extensions,
		IReadOnlySet<string> previousSelections)
	{
		var list = new List<SelectionOption>();
		var ordered = extensions.OrderBy(e => e, StringComparer.OrdinalIgnoreCase).ToList();
		foreach (var ext in ordered)
		{
			bool isChecked = previousSelections.Contains(ext);

			list.Add(new SelectionOption(ext, isChecked));
		}

		return list;
	}

	public IReadOnlyList<SelectionOption> BuildRootFolderOptions(
		IEnumerable<string> rootFolders,
		IReadOnlySet<string> previousSelections,
		IgnoreRules ignoreRules)
	{
		var list = new List<SelectionOption>();
		bool hasPrevious = previousSelections.Count > 0;

		foreach (var name in rootFolders)
		{
			bool isChecked = previousSelections.Contains(name) ||
				(!hasPrevious && !IsIgnoredByRules(name, ignoreRules));

			list.Add(new SelectionOption(name, isChecked));
		}

		return list;
	}

	private static bool IsIgnoredByRules(string name, IgnoreRules rules)
	{
		if (rules.SmartIgnoredFolders.Contains(name))
			return true;

		if (rules.IgnoreBinFolders && name.Equals("bin", StringComparison.OrdinalIgnoreCase))
			return true;

		if (rules.IgnoreObjFolders && name.Equals("obj", StringComparison.OrdinalIgnoreCase))
			return true;

		if (rules.IgnoreDotFolders && name.StartsWith(".", StringComparison.Ordinal))
			return true;

		return false;
	}
}
