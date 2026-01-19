using System.Collections.Generic;
using ProjectTreeViewer.Application.Services;
using ProjectTreeViewer.Kernel.Models;
using ProjectTreeViewer.Tests.Unit.Helpers;
using Xunit;

namespace ProjectTreeViewer.Tests.Unit;

public sealed class TreeNodePresentationServiceTests
{
	// Verifies access-denied nodes use localized display names and icons.
	[Fact]
	public void Build_UsesLocalizationForAccessDenied()
	{
		var catalog = new StubLocalizationCatalog(new Dictionary<AppLanguage, IReadOnlyDictionary<string, string>>
		{
			[AppLanguage.En] = new Dictionary<string, string>
			{
				["Tree.AccessDeniedRoot"] = "RootDenied",
				["Tree.AccessDenied"] = "ChildDenied"
			}
		});
		var localization = new LocalizationService(catalog, AppLanguage.En);
		var iconMapper = new StubIconMapper { IconKey = "icon" };
		var service = new TreeNodePresentationService(localization, iconMapper);

		var root = new FileSystemNode(
			name: "root",
			fullPath: "/root",
			isDirectory: true,
			isAccessDenied: true,
			children: new List<FileSystemNode>
			{
				new FileSystemNode("child", "/root/child", true, true, new List<FileSystemNode>())
			});

		var result = service.Build(root);

		Assert.Equal("RootDenied", result.DisplayName);
		Assert.Equal("ChildDenied", result.Children[0].DisplayName);
		Assert.Equal("icon", result.IconKey);
	}
}
