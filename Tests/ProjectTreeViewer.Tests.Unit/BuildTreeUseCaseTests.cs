using System.Collections.Generic;
using ProjectTreeViewer.Application.Services;
using ProjectTreeViewer.Application.UseCases;
using ProjectTreeViewer.Kernel.Contracts;
using ProjectTreeViewer.Kernel.Models;
using ProjectTreeViewer.Tests.Unit.Helpers;
using Xunit;

namespace ProjectTreeViewer.Tests.Unit;

public sealed class BuildTreeUseCaseTests
{
	[Fact]
	public void Execute_ReturnsPresentedTree()
	{
		var treeBuilder = new StubTreeBuilder
		{
			Result = new TreeBuildResult(
				new FileSystemNode("root", "/root", true, false, new List<FileSystemNode>()),
				rootAccessDenied: false,
				hadAccessDenied: false)
		};

		var catalog = new StubLocalizationCatalog(new Dictionary<AppLanguage, IReadOnlyDictionary<string, string>>
		{
			[AppLanguage.En] = new Dictionary<string, string>()
		});
		var localization = new LocalizationService(catalog, AppLanguage.En);
		var presenter = new TreeNodePresentationService(localization, new StubIconMapper { IconKey = "folder" });

		var useCase = new BuildTreeUseCase(treeBuilder, presenter);

		var result = useCase.Execute(new BuildTreeRequest("/root", new TreeFilterOptions(
			AllowedExtensions: new HashSet<string>(),
			AllowedRootFolders: new HashSet<string>(),
			IgnoreRules: new IgnoreRules(
				IgnoreBinFolders: false,
				IgnoreObjFolders: false,
				IgnoreHiddenFolders: false,
				IgnoreHiddenFiles: false,
				IgnoreDotFolders: false,
				IgnoreDotFiles: false,
				SmartIgnoredFolders: new HashSet<string>(),
				SmartIgnoredFiles: new HashSet<string>()))));

		Assert.Equal("root", result.Root.DisplayName);
		Assert.Equal("folder", result.Root.IconKey);
	}
}
