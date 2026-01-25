using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProjectTreeViewer.Application.Services;
using ProjectTreeViewer.Application.UseCases;
using ProjectTreeViewer.Avalonia.Coordinators;
using ProjectTreeViewer.Avalonia.ViewModels;
using ProjectTreeViewer.Infrastructure.ResourceStore;
using ProjectTreeViewer.Kernel.Models;
using ProjectTreeViewer.Tests.Unit.Helpers;
using Xunit;

namespace ProjectTreeViewer.Tests.Unit;

public sealed class SelectionSyncCoordinatorAdditionalTests
{
	[Fact]
	public void HandleRootAllChanged_ChecksAllRootFolderOptions()
	{
		var viewModel = CreateViewModel();
		viewModel.RootFolders.Add(new SelectionOptionViewModel("src", false));
		viewModel.RootFolders.Add(new SelectionOptionViewModel("tests", false));

		var coordinator = CreateCoordinator(viewModel);

		coordinator.HandleRootAllChanged(true, currentPath: null);

		Assert.True(viewModel.AllRootFoldersChecked);
		Assert.All(viewModel.RootFolders, option => Assert.True(option.IsChecked));
	}

	[Fact]
	public void HandleExtensionsAllChanged_ChecksAllExtensionOptions()
	{
		var viewModel = CreateViewModel();
		viewModel.Extensions.Add(new SelectionOptionViewModel(".cs", false));
		viewModel.Extensions.Add(new SelectionOptionViewModel(".md", false));

		var coordinator = CreateCoordinator(viewModel);

		coordinator.HandleExtensionsAllChanged(true);

		Assert.True(viewModel.AllExtensionsChecked);
		Assert.All(viewModel.Extensions, option => Assert.True(option.IsChecked));
	}

	[Fact]
	public void HandleIgnoreAllChanged_ChecksAllIgnoreOptions()
	{
		var viewModel = CreateViewModel();
		viewModel.IgnoreOptions.Add(new IgnoreOptionViewModel(IgnoreOptionId.BinFolders, "bin", false));
		viewModel.IgnoreOptions.Add(new IgnoreOptionViewModel(IgnoreOptionId.ObjFolders, "obj", false));

		var coordinator = CreateCoordinator(viewModel);

		coordinator.HandleIgnoreAllChanged(true, currentPath: null);

		Assert.True(viewModel.AllIgnoreChecked);
		Assert.All(viewModel.IgnoreOptions, option => Assert.True(option.IsChecked));
	}

	[Fact]
	public async Task PopulateExtensionsForRootSelectionAsync_EmptyRoots_ClearsExtensions()
	{
		var viewModel = CreateViewModel();
		viewModel.Extensions.Add(new SelectionOptionViewModel(".cs", true));
		viewModel.Extensions.Add(new SelectionOptionViewModel(".md", true));
		viewModel.AllExtensionsChecked = true;

		var coordinator = CreateCoordinator(viewModel);

		await coordinator.PopulateExtensionsForRootSelectionAsync("root", new List<string>());

		Assert.Empty(viewModel.Extensions);
		Assert.False(viewModel.AllExtensionsChecked);
	}

	[Fact]
	public async Task PopulateExtensionsForRootSelectionAsync_EmptyPath_DoesNotChangeExtensions()
	{
		var viewModel = CreateViewModel();
		viewModel.Extensions.Add(new SelectionOptionViewModel(".cs", true));

		var coordinator = CreateCoordinator(viewModel);

		await coordinator.PopulateExtensionsForRootSelectionAsync(string.Empty, new List<string> { "src" });

		Assert.Single(viewModel.Extensions);
		Assert.Equal(".cs", viewModel.Extensions[0].Name);
	}

	[Fact]
	public async Task PopulateRootFoldersAsync_EmptyPath_DoesNotChangeRootFolders()
	{
		var viewModel = CreateViewModel();
		viewModel.RootFolders.Add(new SelectionOptionViewModel("src", true));

		var coordinator = CreateCoordinator(viewModel);

		await coordinator.PopulateRootFoldersAsync(string.Empty);

		Assert.Single(viewModel.RootFolders);
		Assert.Equal("src", viewModel.RootFolders[0].Name);
	}

	[Fact]
	public async Task UpdateLiveOptionsFromRootSelectionAsync_EmptyPath_DoesNotChangeOptions()
	{
		var viewModel = CreateViewModel();
		viewModel.Extensions.Add(new SelectionOptionViewModel(".cs", true));
		viewModel.IgnoreOptions.Add(new IgnoreOptionViewModel(IgnoreOptionId.BinFolders, "bin", true));

		var coordinator = CreateCoordinator(viewModel);

		await coordinator.UpdateLiveOptionsFromRootSelectionAsync(null);

		Assert.Single(viewModel.Extensions);
		Assert.Single(viewModel.IgnoreOptions);
	}

	private static MainWindowViewModel CreateViewModel()
	{
		var localization = new LocalizationService(CreateCatalog(), AppLanguage.En);
		return new MainWindowViewModel(localization, new HelpContentProvider());
	}

	private static SelectionSyncCoordinator CreateCoordinator(MainWindowViewModel viewModel)
	{
		var localization = new LocalizationService(CreateCatalog(), AppLanguage.En);
		var scanner = new StubFileSystemScanner();
		var scanOptions = new ScanOptionsUseCase(scanner);
		var filterService = new FilterOptionSelectionService();
		var ignoreService = new IgnoreOptionsService(localization);
		Func<string, IgnoreRules> buildIgnoreRules = _ => new IgnoreRules(
			false,
			false,
			false,
			false,
			false,
			false,
			new HashSet<string>(),
			new HashSet<string>());

		return new SelectionSyncCoordinator(
			viewModel,
			scanOptions,
			filterService,
			ignoreService,
			buildIgnoreRules,
			_ => false,
			() => null);
	}

	private static StubLocalizationCatalog CreateCatalog()
	{
		var data = new Dictionary<AppLanguage, IReadOnlyDictionary<string, string>>
		{
			[AppLanguage.En] = new Dictionary<string, string>
			{
				["Settings.Ignore.BinFolders"] = "bin folders",
				["Settings.Ignore.ObjFolders"] = "obj folders",
				["Settings.Ignore.HiddenFolders"] = "Hidden folders",
				["Settings.Ignore.HiddenFiles"] = "Hidden files",
				["Settings.Ignore.DotFolders"] = "dot folders",
				["Settings.Ignore.DotFiles"] = "dot files"
			}
		};

		return new StubLocalizationCatalog(data);
	}
}
