using System.Collections.Generic;
using ProjectTreeViewer.Kernel.Abstractions;
using ProjectTreeViewer.Kernel.Models;

namespace ProjectTreeViewer.Application.Services;

public sealed class IgnoreOptionsService
{
	private readonly LocalizationService _localization;
	private readonly ISmartIgnoreAnalyzer _analyzer;

	public IgnoreOptionsService(LocalizationService localization, ISmartIgnoreAnalyzer analyzer)
	{
		_localization = localization;
		_analyzer = analyzer;
	}

	public IReadOnlyList<IgnoreOptionDescriptor> GetOptions(string rootPath)
	{
		var definitions = _analyzer.Analyze(rootPath);
		var list = new List<IgnoreOptionDescriptor>(definitions.Count);

		foreach (var definition in definitions)
		{
			var label = definition.Kind switch
			{
				IgnoreOptionKind.NamedFolder => _localization.Format("Settings.Ignore.NamedFolder", definition.Id),
				IgnoreOptionKind.NamedFile => _localization.Format("Settings.Ignore.NamedFile", definition.Id),
				IgnoreOptionKind.HiddenFolders => _localization["Settings.Ignore.HiddenFolders"],
				IgnoreOptionKind.HiddenFiles => _localization["Settings.Ignore.HiddenFiles"],
				IgnoreOptionKind.DotFolders => _localization["Settings.Ignore.DotFolders"],
				IgnoreOptionKind.DotFiles => _localization["Settings.Ignore.DotFiles"],
				_ => definition.Id
			};

			list.Add(new IgnoreOptionDescriptor(definition.Id, definition.Kind, label, definition.DefaultChecked));
		}

		return list;
	}
}
