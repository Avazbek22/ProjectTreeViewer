using System;
using System.Collections.Generic;
using System.Linq;
using ProjectTreeViewer.Application.Models;
using ProjectTreeViewer.Kernel.Abstractions;
using ProjectTreeViewer.Kernel.Models;

namespace ProjectTreeViewer.Application.Services;

public sealed class FilterOptionSelectionService
{
	private readonly HashSet<string> _defaultExtensions;

	public FilterOptionSelectionService(IDefaultExtensionCatalog catalog)
	{
		_defaultExtensions = new HashSet<string>(catalog.GetDefaultExtensions(), StringComparer.OrdinalIgnoreCase);
	}

	public IReadOnlyList<SelectionOption> BuildExtensionOptions(
		IEnumerable<string> extensions,
		IReadOnlySet<string> previousSelections)
	{
		var list = new List<SelectionOption>();
		bool hasPrevious = previousSelections.Count > 0;

		foreach (var ext in extensions.OrderBy(e => e, StringComparer.OrdinalIgnoreCase))
		{
			bool isChecked = previousSelections.Contains(ext) ||
				(!hasPrevious && _defaultExtensions.Contains(ext));

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

		if (rules.IgnoreDotFolders && name.StartsWith(".", StringComparison.Ordinal))
			return true;

		return false;
	}
}
