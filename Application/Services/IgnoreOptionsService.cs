using System.Collections.Generic;
using ProjectTreeViewer.Kernel.Models;

namespace ProjectTreeViewer.Application.Services;

public sealed class IgnoreOptionsService
{
	private readonly LocalizationService _localization;

	public IgnoreOptionsService(LocalizationService localization)
	{
		_localization = localization;
	}

	public IReadOnlyList<IgnoreOptionDescriptor> GetOptions()
	{
		return new[]
		{
			new IgnoreOptionDescriptor(IgnoreOptionId.BinFolders, _localization["Settings.Ignore.BinFolders"], true),
			new IgnoreOptionDescriptor(IgnoreOptionId.ObjFolders, _localization["Settings.Ignore.ObjFolders"], true),
			new IgnoreOptionDescriptor(IgnoreOptionId.HiddenFolders, _localization["Settings.Ignore.HiddenFolders"], true),
			new IgnoreOptionDescriptor(IgnoreOptionId.HiddenFiles, _localization["Settings.Ignore.HiddenFiles"], true),
			new IgnoreOptionDescriptor(IgnoreOptionId.DotFolders, _localization["Settings.Ignore.DotFolders"], true),
			new IgnoreOptionDescriptor(IgnoreOptionId.DotFiles, _localization["Settings.Ignore.DotFiles"], true)
		};
	}
}
