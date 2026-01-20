using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using ProjectTreeViewer.Application.Services;
using ProjectTreeViewer.Application.UseCases;
using ProjectTreeViewer.Avalonia.Services;
using ProjectTreeViewer.Avalonia.ViewModels;
using ProjectTreeViewer.Kernel.Abstractions;
using ProjectTreeViewer.Kernel.Contracts;
using ProjectTreeViewer.Kernel.Models;

namespace ProjectTreeViewer.Avalonia;

public partial class MainWindow : Window
{
    private readonly CommandLineOptions _startupOptions;
    private readonly LocalizationService _localization;
    private readonly ScanOptionsUseCase _scanOptions;
    private readonly BuildTreeUseCase _buildTree;
    private readonly IgnoreOptionsService _ignoreOptionsService;
    private readonly IgnoreRulesService _ignoreRulesService;
    private readonly FilterOptionSelectionService _filterSelectionService;
    private readonly TreeExportService _treeExport;
    private readonly SelectedContentExportService _contentExport;
    private readonly TreeAndContentExportService _treeAndContentExport;
    private readonly IconCache _iconCache;
    private readonly IElevationService _elevation;

    private readonly MainWindowViewModel _viewModel;

    private BuildTreeResult? _currentTree;
    private string? _currentPath;
    private bool _elevationAttempted;

    private IReadOnlyList<IgnoreOptionDescriptor> _ignoreOptions = Array.Empty<IgnoreOptionDescriptor>();
    private HashSet<IgnoreOptionId> _ignoreSelectionCache = new();
    private bool _ignoreSelectionInitialized;
    private HashSet<string> _extensionsSelectionCache = new(StringComparer.OrdinalIgnoreCase);

    private readonly List<TreeNodeViewModel> _searchMatches = new();
    private int _searchMatchIndex = -1;
    private TextBox? _searchBox;
    private TreeView? _treeView;

    public MainWindow(CommandLineOptions startupOptions, AvaloniaAppServices services)
    {
        _startupOptions = startupOptions;
        _localization = services.Localization;
        _scanOptions = services.ScanOptionsUseCase;
        _buildTree = services.BuildTreeUseCase;
        _ignoreOptionsService = services.IgnoreOptionsService;
        _ignoreRulesService = services.IgnoreRulesService;
        _filterSelectionService = services.FilterOptionSelectionService;
        _treeExport = services.TreeExportService;
        _contentExport = services.ContentExportService;
        _treeAndContentExport = services.TreeAndContentExportService;
        _iconCache = new IconCache(services.IconStore);
        _elevation = services.Elevation;

        _viewModel = new MainWindowViewModel(_localization);
        DataContext = _viewModel;

        InitializeComponent();

        _searchBox = this.FindControl<TextBox>("SearchBox");
        _treeView = this.FindControl<TreeView>("ProjectTree");

        if (_treeView is not null)
        {
            _treeView.PointerEntered += OnTreePointerEntered;
            _treeView.PointerWheelChanged += OnTreePointerWheelChanged;
        }

        _elevationAttempted = startupOptions.ElevationAttempted;

        _localization.LanguageChanged += (_, _) => ApplyLocalization();
        var app = global::Avalonia.Application.Current;
        if (app is not null)
            app.ActualThemeVariantChanged += OnThemeChanged;

        InitializeFonts();
        HookOptionListeners(_viewModel.RootFolders);
        HookOptionListeners(_viewModel.Extensions);
        HookIgnoreListeners(_viewModel.IgnoreOptions);

        _viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(MainWindowViewModel.SearchQuery))
                UpdateSearchMatches();
        };

        AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);

        Opened += OnOpened;
    }

    private void OnThemeChanged(object? sender, EventArgs e)
    {
        UpdateSearchHighlights(_viewModel.SearchQuery);
    }

    private async void OnOpened(object? sender, EventArgs e)
    {
        SyncThemeWithSystem();

        if (!string.IsNullOrWhiteSpace(_startupOptions.Path))
            TryOpenFolder(_startupOptions.Path!, fromDialog: false);
    }

    private void InitializeFonts()
    {
        var fonts = FontManager.Current?.SystemFonts?.Select(f => f.Name).Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList() ?? new List<string>();

        foreach (var font in fonts)
            _viewModel.FontFamilies.Add(font);

        var preferred = new[] { "Consolas", "Courier New", "Fira Code", "Lucida Console" };
        var selected = preferred.FirstOrDefault(name => fonts.Contains(name, StringComparer.OrdinalIgnoreCase))
            ?? fonts.FirstOrDefault();

        _viewModel.SelectedFontFamily = selected;
        _viewModel.PendingFontFamily = selected;
    }

    private void SyncThemeWithSystem()
    {
        var app = global::Avalonia.Application.Current;
        if (app is null) return;

        var isDark = app.ActualThemeVariant == ThemeVariant.Dark;
        _viewModel.IsDarkTheme = isDark;
    }

    private void ApplyLocalization()
    {
        _viewModel.UpdateLocalization();
        UpdateTitle();

        if (_currentPath is not null)
        {
            PopulateIgnoreOptionsForRootSelection(GetSelectedRootFolders());
        }
    }

    private async Task ShowErrorAsync(string message) =>
        await MessageDialog.ShowAsync(this, _localization["Msg.ErrorTitle"], message);

    private async Task ShowInfoAsync(string message) =>
        await MessageDialog.ShowAsync(this, _localization["Msg.InfoTitle"], message);

    private async void OnOpenFolder(object? sender, RoutedEventArgs e)
    {
        var options = new FolderPickerOpenOptions
        {
            AllowMultiple = false,
            Title = _viewModel.MenuFileOpen
        };

        var folders = await StorageProvider.OpenFolderPickerAsync(options);
        var folder = folders.FirstOrDefault();
        var path = folder?.TryGetLocalPath();
        if (string.IsNullOrWhiteSpace(path))
            return;

        TryOpenFolder(path, fromDialog: true);
    }

    private async void OnRefresh(object? sender, RoutedEventArgs e)
    {
        try
        {
            ReloadProject();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex.Message);
        }
    }

    private void OnExit(object? sender, RoutedEventArgs e) => Close();

    private async void OnCopyFullTree(object? sender, RoutedEventArgs e)
    {
        if (!EnsureTreeReady()) return;

        var content = _treeExport.BuildFullTree(_currentPath!, _currentTree!.Root);
        await SetClipboardTextAsync(content);
    }

    private async void OnCopySelectedTree(object? sender, RoutedEventArgs e)
    {
        if (!EnsureTreeReady()) return;

        var selected = GetCheckedPaths();
        var content = _treeExport.BuildSelectedTree(_currentPath!, _currentTree!.Root, selected);
        if (string.IsNullOrWhiteSpace(content))
        {
            await ShowInfoAsync(_localization["Msg.NoCheckedTree"]);
            return;
        }

        await SetClipboardTextAsync(content);
    }

    private async void OnCopySelectedContent(object? sender, RoutedEventArgs e)
    {
        if (!EnsureTreeReady()) return;

        var selected = GetCheckedPaths();
        var files = selected.Where(File.Exists)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (files.Count == 0)
        {
            await ShowInfoAsync(_localization["Msg.NoCheckedFiles"]);
            return;
        }

        var content = _contentExport.Build(files);
        if (string.IsNullOrWhiteSpace(content))
        {
            await ShowInfoAsync(_localization["Msg.NoTextContent"]);
            return;
        }

        await SetClipboardTextAsync(content);
    }

    private async void OnCopyTreeAndContent(object? sender, RoutedEventArgs e)
    {
        if (!EnsureTreeReady()) return;

        var selected = GetCheckedPaths();
        var content = _treeAndContentExport.Build(_currentPath!, _currentTree!.Root, selected);
        await SetClipboardTextAsync(content);
    }

    private void OnExpandAll(object? sender, RoutedEventArgs e) => ExpandCollapseTree(expand: true);

    private void OnCollapseAll(object? sender, RoutedEventArgs e) => ExpandCollapseTree(expand: false);

    private void ExpandCollapseTree(bool expand)
    {
        foreach (var node in _viewModel.TreeNodes)
        {
            node.SetExpandedRecursive(expand);
            if (!expand)
                node.IsExpanded = true;
        }
    }

    private void OnZoomIn(object? sender, RoutedEventArgs e) => AdjustTreeFontSize(1);

    private void OnZoomOut(object? sender, RoutedEventArgs e) => AdjustTreeFontSize(-1);

    private void OnZoomReset(object? sender, RoutedEventArgs e) => _viewModel.TreeFontSize = 9;

    private void AdjustTreeFontSize(double delta)
    {
        const double min = 6;
        const double max = 28;
        var next = Math.Clamp(_viewModel.TreeFontSize + delta, min, max);
        _viewModel.TreeFontSize = next;
    }

    private void OnToggleSettings(object? sender, RoutedEventArgs e)
    {
        if (!_viewModel.IsProjectLoaded) return;
        _viewModel.SettingsVisible = !_viewModel.SettingsVisible;
    }

    private void OnToggleTheme(object? sender, RoutedEventArgs e)
    {
        var app = global::Avalonia.Application.Current;
        if (app is null) return;

        var nextIsDark = !_viewModel.IsDarkTheme;
        app.RequestedThemeVariant = nextIsDark ? ThemeVariant.Dark : ThemeVariant.Light;
        _viewModel.IsDarkTheme = nextIsDark;
        UpdateSearchHighlights(_viewModel.SearchQuery);
    }

    private void OnLangRu(object? sender, RoutedEventArgs e) => _localization.SetLanguage(AppLanguage.Ru);
    private void OnLangEn(object? sender, RoutedEventArgs e) => _localization.SetLanguage(AppLanguage.En);
    private void OnLangUz(object? sender, RoutedEventArgs e) => _localization.SetLanguage(AppLanguage.Uz);
    private void OnLangTg(object? sender, RoutedEventArgs e) => _localization.SetLanguage(AppLanguage.Tg);
    private void OnLangKk(object? sender, RoutedEventArgs e) => _localization.SetLanguage(AppLanguage.Kk);
    private void OnLangFr(object? sender, RoutedEventArgs e) => _localization.SetLanguage(AppLanguage.Fr);
    private void OnLangDe(object? sender, RoutedEventArgs e) => _localization.SetLanguage(AppLanguage.De);
    private void OnLangIt(object? sender, RoutedEventArgs e) => _localization.SetLanguage(AppLanguage.It);

    private async void OnAbout(object? sender, RoutedEventArgs e)
    {
        await ShowInfoAsync(_localization["Msg.AboutStub"]);
    }

    private void OnSearchNext(object? sender, RoutedEventArgs e) => NavigateSearch(1);

    private void OnSearchPrev(object? sender, RoutedEventArgs e) => NavigateSearch(-1);

    private void OnToggleSearch(object? sender, RoutedEventArgs e)
    {
        if (!_viewModel.IsProjectLoaded) return;

        if (_viewModel.SearchVisible)
        {
            CloseSearch();
            return;
        }

        ShowSearch();
    }

    private void OnSearchClose(object? sender, RoutedEventArgs e) => CloseSearch();

    private void OnSearchKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            CloseSearch();
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Enter)
        {
            if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
                NavigateSearch(-1);
            else
                NavigateSearch(1);

            e.Handled = true;
        }
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        var mods = e.KeyModifiers;

        // Ctrl+O (всегда доступно)
        if (mods == KeyModifiers.Control && e.Key == Key.O)
        {
            OnOpenFolder(this, new RoutedEventArgs());
            e.Handled = true;
            return;
        }

        // Ctrl+F (только при загруженном проекте — как WinForms miSearch.Enabled)
        if (mods == KeyModifiers.Control && e.Key == Key.F)
        {
            OnToggleSearch(this, new RoutedEventArgs());
            e.Handled = true;
            return;
        }

        // Esc закрывает поиск
        if (e.Key == Key.Escape && _viewModel.SearchVisible)
        {
            CloseSearch();
            e.Handled = true;
            return;
        }

        // F5 refresh (как WinForms)
        if (e.Key == Key.F5)
        {
            if (_viewModel.IsProjectLoaded)
                OnRefresh(this, new RoutedEventArgs());

            e.Handled = true;
            return;
        }

        // Zoom горячие клавиши (в WinForms работают даже без проекта)
        if (mods == KeyModifiers.Control && (e.Key == Key.OemPlus || e.Key == Key.Add))
        {
            AdjustTreeFontSize(1);
            e.Handled = true;
            return;
        }

        if (mods == KeyModifiers.Control && (e.Key == Key.OemMinus || e.Key == Key.Subtract))
        {
            AdjustTreeFontSize(-1);
            e.Handled = true;
            return;
        }

        if (mods == KeyModifiers.Control && (e.Key == Key.D0 || e.Key == Key.NumPad0))
        {
            OnZoomReset(this, new RoutedEventArgs());
            e.Handled = true;
            return;
        }

        if (!_viewModel.IsProjectLoaded)
            return;

        // Ctrl+P Options panel toggle
        if (mods == KeyModifiers.Control && e.Key == Key.P)
        {
            OnToggleSettings(this, new RoutedEventArgs());
            e.Handled = true;
            return;
        }

        // Ctrl+E Expand All
        if (mods == KeyModifiers.Control && e.Key == Key.E)
        {
            ExpandCollapseTree(expand: true);
            e.Handled = true;
            return;
        }

        // Ctrl+W Collapse All
        if (mods == KeyModifiers.Control && e.Key == Key.W)
        {
            ExpandCollapseTree(expand: false);
            e.Handled = true;
            return;
        }

        // Copy hotkeys (как WinForms)
        if (mods == (KeyModifiers.Control | KeyModifiers.Shift) && e.Key == Key.C)
        {
            OnCopyFullTree(this, new RoutedEventArgs());
            e.Handled = true;
            return;
        }

        if (mods == (KeyModifiers.Control | KeyModifiers.Alt) && e.Key == Key.C)
        {
            OnCopySelectedTree(this, new RoutedEventArgs());
            e.Handled = true;
            return;
        }

        if (mods == (KeyModifiers.Control | KeyModifiers.Alt) && e.Key == Key.V)
        {
            OnCopySelectedContent(this, new RoutedEventArgs());
            e.Handled = true;
            return;
        }

        if (mods == (KeyModifiers.Control | KeyModifiers.Shift) && e.Key == Key.V)
        {
            OnCopyTreeAndContent(this, new RoutedEventArgs());
            e.Handled = true;
            return;
        }
    }

    private void OnTreePointerEntered(object? sender, PointerEventArgs e)
    {
        _treeView?.Focus();
    }

    private void OnTreePointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (!e.KeyModifiers.HasFlag(KeyModifiers.Control))
            return;

        if (e.Delta.Y > 0)
            AdjustTreeFontSize(1);
        else if (e.Delta.Y < 0)
            AdjustTreeFontSize(-1);

        // В WinForms нельзя "handled", поэтому оставляем скролл как есть (бесшовно по ощущению).
    }

    private void ShowSearch()
    {
        if (!_viewModel.IsProjectLoaded) return;

        _viewModel.SearchVisible = true;
        _searchBox?.Focus();
        _searchBox?.SelectAll();
    }

    private void CloseSearch()
    {
        if (!_viewModel.SearchVisible) return;

        _viewModel.SearchVisible = false;
        _viewModel.SearchQuery = string.Empty;
        ClearSearchState();
        _treeView?.Focus();
    }

    private void NavigateSearch(int step)
    {
        if (_searchMatches.Count == 0)
            return;

        _searchMatchIndex = (_searchMatchIndex + step + _searchMatches.Count) % _searchMatches.Count;
        SelectSearchMatch();
    }

    private void SelectSearchMatch()
    {
        if (_searchMatchIndex < 0 || _searchMatchIndex >= _searchMatches.Count)
            return;

        var node = _searchMatches[_searchMatchIndex];
        node.EnsureParentsExpanded();
        SelectTreeNode(node);
        BringNodeIntoView(node);
        _treeView?.Focus();
    }

    private void UpdateSearchMatches()
    {
        _searchMatches.Clear();
        _searchMatchIndex = -1;

        var query = _viewModel.SearchQuery;
        UpdateSearchHighlights(query);
        if (string.IsNullOrWhiteSpace(query))
        {
            return;
        }

        foreach (var node in _viewModel.TreeNodes.SelectMany(n => n.Flatten()))
        {
            if (node.DisplayName.Contains(query, StringComparison.OrdinalIgnoreCase))
                _searchMatches.Add(node);
        }

        if (_searchMatches.Count > 0)
        {
            _searchMatchIndex = 0;
            SelectSearchMatch();
        }
    }

    private void ClearSearchState()
    {
        _searchMatches.Clear();
        _searchMatchIndex = -1;
        UpdateSearchHighlights(string.Empty);
    }

    private void UpdateSearchHighlights(string? query)
    {
        var (background, foreground) = GetSearchHighlightBrushes();
        foreach (var node in _viewModel.TreeNodes.SelectMany(n => n.Flatten()))
            node.UpdateSearchHighlight(query, background, foreground);
    }

    private (IBrush? background, IBrush? foreground) GetSearchHighlightBrushes()
    {
        var app = global::Avalonia.Application.Current;
        var theme = app?.ActualThemeVariant;

        IBrush? background = null;
        IBrush? foreground = null;

        if (app?.Resources.TryGetResource("TreeSearchHighlightBrush", theme, out var bg) == true)
            background = bg as IBrush;

        if (app?.Resources.TryGetResource("TreeSearchHighlightTextBrush", theme, out var fg) == true)
            foreground = fg as IBrush;

        if (foreground is null && app?.Resources.TryGetResource("AppTextBrush", theme, out var textFg) == true)
            foreground = textFg as IBrush;

        background ??= new SolidColorBrush(Color.Parse("#FFEB3B"));

        foreground ??= theme == ThemeVariant.Dark
            ? new SolidColorBrush(Color.Parse("#E7E9EF"))
            : new SolidColorBrush(Color.Parse("#1A1A1A"));

        return (background, foreground);
    }

    private void BringNodeIntoView(TreeNodeViewModel node)
    {
        var item = _treeView?.GetLogicalDescendants()
            .OfType<TreeViewItem>()
            .FirstOrDefault(container => ReferenceEquals(container.DataContext, node));

        item?.BringIntoView();
    }

    private void SelectTreeNode(TreeNodeViewModel node)
    {
        if (_treeView is not null)
            _treeView.SelectedItem = node;

        node.IsSelected = true;
    }

    private void OnRootAllChanged(object? sender, RoutedEventArgs e)
    {
        if (_suppressRootAllCheck) return;

        var check = _viewModel.AllRootFoldersChecked;
        SetAllChecked(_viewModel.RootFolders, check, ref _suppressRootItemCheck);
        UpdateLiveOptionsFromRootSelection();
    }

    private void OnExtensionsAllChanged(object? sender, RoutedEventArgs e)
    {
        if (_suppressExtensionAllCheck) return;

        var check = _viewModel.AllExtensionsChecked;
        SetAllChecked(_viewModel.Extensions, check, ref _suppressExtensionItemCheck);
        UpdateExtensionsSelectionCache();
    }

    private void OnIgnoreAllChanged(object? sender, RoutedEventArgs e)
    {
        if (_suppressIgnoreAllCheck) return;

        _ignoreSelectionInitialized = true;
        var check = _viewModel.AllIgnoreChecked;
        SetAllChecked(_viewModel.IgnoreOptions, check, ref _suppressIgnoreItemCheck);
        UpdateIgnoreSelectionCache();
        PopulateRootFolders(_currentPath ?? string.Empty);
        UpdateLiveOptionsFromRootSelection();
    }

    private bool _suppressRootAllCheck;
    private bool _suppressRootItemCheck;
    private bool _suppressExtensionAllCheck;
    private bool _suppressExtensionItemCheck;
    private bool _suppressIgnoreAllCheck;
    private bool _suppressIgnoreItemCheck;

    private void HookOptionListeners(ObservableCollection<SelectionOptionViewModel> options)
    {
        options.CollectionChanged += (_, _) =>
        {
            foreach (var item in options)
                item.CheckedChanged -= OnOptionCheckedChanged;
            foreach (var item in options)
                item.CheckedChanged += OnOptionCheckedChanged;
        };
    }

    private void HookIgnoreListeners(ObservableCollection<IgnoreOptionViewModel> options)
    {
        options.CollectionChanged += (_, _) =>
        {
            foreach (var item in options)
                item.CheckedChanged -= OnIgnoreCheckedChanged;
            foreach (var item in options)
                item.CheckedChanged += OnIgnoreCheckedChanged;
        };
    }

    private void OnOptionCheckedChanged(object? sender, EventArgs e)
    {
        if (sender is not SelectionOptionViewModel option)
            return;

        if (_viewModel.RootFolders.Contains(option))
        {
            if (_suppressRootItemCheck) return;

            SyncAllCheckbox(_viewModel.RootFolders, ref _suppressRootAllCheck,
                value => _viewModel.AllRootFoldersChecked = value);

            UpdateLiveOptionsFromRootSelection();
        }
        else if (_viewModel.Extensions.Contains(option))
        {
            if (_suppressExtensionItemCheck) return;

            SyncAllCheckbox(_viewModel.Extensions, ref _suppressExtensionAllCheck,
                value => _viewModel.AllExtensionsChecked = value);

            UpdateExtensionsSelectionCache();
        }
    }

    private void OnIgnoreCheckedChanged(object? sender, EventArgs e)
    {
        if (_suppressIgnoreItemCheck) return;

        _ignoreSelectionInitialized = true;

        SyncAllCheckbox(_viewModel.IgnoreOptions, ref _suppressIgnoreAllCheck,
            value => _viewModel.AllIgnoreChecked = value);

        UpdateIgnoreSelectionCache();
        PopulateRootFolders(_currentPath ?? string.Empty);
        UpdateLiveOptionsFromRootSelection();
    }

    private async void OnApplySettings(object? sender, RoutedEventArgs e)
    {
        try
        {
            // Font family — как WinForms: применяется только по Apply
            if (!string.IsNullOrWhiteSpace(_viewModel.PendingFontFamily) &&
                !string.Equals(_viewModel.SelectedFontFamily, _viewModel.PendingFontFamily, StringComparison.Ordinal))
            {
                _viewModel.SelectedFontFamily = _viewModel.PendingFontFamily;
            }

            RefreshTree();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex.Message);
        }
    }

    private void TryOpenFolder(string path, bool fromDialog)
    {
        if (!Directory.Exists(path))
        {
            _ = ShowErrorAsync(_localization.Format("Msg.PathNotFound", path));
            return;
        }

        if (!_scanOptions.CanReadRoot(path))
        {
            if (TryElevateAndRestart(path))
                return;

            _ = ShowErrorAsync(_localization["Msg.AccessDeniedRoot"]);
            return;
        }

        _currentPath = path;
        _viewModel.IsProjectLoaded = true;
        _viewModel.SettingsVisible = true;
        _viewModel.SearchVisible = false;
        UpdateTitle();

        ReloadProject();
    }

    private bool TryElevateAndRestart(string path)
    {
        if (_elevation.IsAdministrator) return false;
        if (_elevationAttempted) return false;

        _elevationAttempted = true;

        var opts = new CommandLineOptions(
            Path: path,
            Language: _localization.CurrentLanguage,
            ElevationAttempted: true);

        bool started = _elevation.TryRelaunchAsAdministrator(opts);
        if (started)
        {
            Close();
            return true;
        }

        _ = ShowInfoAsync(_localization["Msg.ElevationCanceled"]);
        return false;
    }

    private void ReloadProject()
    {
        if (string.IsNullOrEmpty(_currentPath)) return;

        PopulateRootFolders(_currentPath);
        UpdateLiveOptionsFromRootSelection();
        RefreshTree();
    }

    private void RefreshTree()
    {
        if (string.IsNullOrEmpty(_currentPath)) return;

        var allowedExt = new HashSet<string>(_viewModel.Extensions.Where(o => o.IsChecked).Select(o => o.Name),
            StringComparer.OrdinalIgnoreCase);
        var allowedRoot = new HashSet<string>(_viewModel.RootFolders.Where(o => o.IsChecked).Select(o => o.Name),
            StringComparer.OrdinalIgnoreCase);

        var ignoreRules = BuildIgnoreRules(_currentPath);

        var options = new TreeFilterOptions(
            AllowedExtensions: allowedExt,
            AllowedRootFolders: allowedRoot,
            IgnoreRules: ignoreRules);

        Cursor = new Cursor(StandardCursorType.Wait);
        try
        {
            var result = _buildTree.Execute(new BuildTreeRequest(_currentPath, options));
            _currentTree = result;

            if (result.RootAccessDenied && TryElevateAndRestart(_currentPath))
                return;

            _viewModel.TreeNodes.Clear();

            var root = BuildTreeViewModel(result.Root, null);

            // Fix как в WinForms EnsureRootNodeVisible
            try
            {
                root.DisplayName = new DirectoryInfo(_currentPath!).Name;
            }
            catch
            {
                // ignore
            }

            _viewModel.TreeNodes.Add(root);
            root.IsExpanded = true;

            UpdateSearchMatches();
        }
        finally
        {
            Cursor = new Cursor(StandardCursorType.Arrow);
        }
    }

    private TreeNodeViewModel BuildTreeViewModel(TreeNodeDescriptor descriptor, TreeNodeViewModel? parent)
    {
        var icon = _iconCache.GetIcon(descriptor.IconKey);
        var node = new TreeNodeViewModel(descriptor, parent, icon);

        foreach (var child in descriptor.Children)
        {
            var childViewModel = BuildTreeViewModel(child, node);
            node.Children.Add(childViewModel);
        }

        return node;
    }

    private void PopulateExtensionsForRootSelection(string path, IReadOnlyCollection<string> rootFolders)
    {
        if (string.IsNullOrEmpty(path)) return;

        var prev = _extensionsSelectionCache.Count > 0
            ? new HashSet<string>(_extensionsSelectionCache, StringComparer.OrdinalIgnoreCase)
            : new HashSet<string>(_viewModel.Extensions.Where(o => o.IsChecked).Select(o => o.Name), StringComparer.OrdinalIgnoreCase);

        if (rootFolders.Count == 0)
        {
            _viewModel.Extensions.Clear();
            _suppressExtensionAllCheck = true;
            _viewModel.AllExtensionsChecked = false;
            _suppressExtensionAllCheck = false;
            SyncAllCheckbox(_viewModel.Extensions, ref _suppressExtensionAllCheck, value => _viewModel.AllExtensionsChecked = value);
            return;
        }

        var ignoreRules = BuildIgnoreRules(path);
        var scan = _scanOptions.GetExtensionsForRootFolders(path, rootFolders, ignoreRules);
        if (scan.RootAccessDenied && TryElevateAndRestart(path))
            return;

        _viewModel.Extensions.Clear();

        _suppressExtensionItemCheck = true;
        var options = _filterSelectionService.BuildExtensionOptions(scan.Value, prev);
        foreach (var option in options)
            _viewModel.Extensions.Add(new SelectionOptionViewModel(option.Name, option.IsChecked));
        _suppressExtensionItemCheck = false;

        if (_viewModel.AllExtensionsChecked)
            SetAllChecked(_viewModel.Extensions, true, ref _suppressExtensionItemCheck);

        SyncAllCheckbox(_viewModel.Extensions, ref _suppressExtensionAllCheck, value => _viewModel.AllExtensionsChecked = value);
        UpdateExtensionsSelectionCache();
    }

    private void PopulateRootFolders(string path)
    {
        if (string.IsNullOrEmpty(path)) return;

        var prev = new HashSet<string>(_viewModel.RootFolders.Where(o => o.IsChecked).Select(o => o.Name),
            StringComparer.OrdinalIgnoreCase);

        var ignoreRules = BuildIgnoreRules(path);
        var scan = _scanOptions.Execute(new ScanOptionsRequest(path, ignoreRules));
        if (scan.RootAccessDenied && TryElevateAndRestart(path))
            return;

        _viewModel.RootFolders.Clear();

        _suppressRootItemCheck = true;
        var options = _filterSelectionService.BuildRootFolderOptions(scan.RootFolders, prev, ignoreRules);
        foreach (var option in options)
            _viewModel.RootFolders.Add(new SelectionOptionViewModel(option.Name, option.IsChecked));
        _suppressRootItemCheck = false;

        if (_viewModel.AllRootFoldersChecked)
            SetAllChecked(_viewModel.RootFolders, true, ref _suppressRootItemCheck);

        SyncAllCheckbox(_viewModel.RootFolders, ref _suppressRootAllCheck, value => _viewModel.AllRootFoldersChecked = value);
    }

    private void PopulateIgnoreOptionsForRootSelection(IReadOnlyCollection<string> rootFolders)
    {
        var previousSelections = _ignoreSelectionCache;

        _suppressIgnoreItemCheck = true;
        try
        {
            _viewModel.IgnoreOptions.Clear();

            if (rootFolders.Count == 0)
            {
                _ignoreOptions = Array.Empty<IgnoreOptionDescriptor>();
                _suppressIgnoreAllCheck = true;
                _viewModel.AllIgnoreChecked = false;
                _suppressIgnoreAllCheck = false;
                return;
            }

            _ignoreOptions = _ignoreOptionsService.GetOptions();
            bool hasPrevious = _ignoreSelectionInitialized;

            foreach (var option in _ignoreOptions)
            {
                bool isChecked = previousSelections.Contains(option.Id) ||
                    (!hasPrevious && option.DefaultChecked);
                _viewModel.IgnoreOptions.Add(new IgnoreOptionViewModel(option.Id, option.Label, isChecked));
            }
        }
        finally
        {
            _suppressIgnoreItemCheck = false;
        }

        if (_viewModel.AllIgnoreChecked)
            SetAllChecked(_viewModel.IgnoreOptions, true, ref _suppressIgnoreItemCheck);

        UpdateIgnoreSelectionCache();
        SyncIgnoreAllCheckbox();
    }

    private IReadOnlyCollection<string> GetSelectedRootFolders()
    {
        return _viewModel.RootFolders.Where(o => o.IsChecked).Select(o => o.Name).ToList();
    }

    private void UpdateLiveOptionsFromRootSelection()
    {
        if (string.IsNullOrEmpty(_currentPath)) return;

        var selectedRoots = GetSelectedRootFolders();
        PopulateExtensionsForRootSelection(_currentPath, selectedRoots);
        PopulateIgnoreOptionsForRootSelection(selectedRoots);
    }

    private void UpdateTitle()
    {
        _viewModel.Title = string.IsNullOrWhiteSpace(_currentPath)
            ? _localization["Title.Default"]
            : _localization.Format("Title.WithPath", _currentPath);
    }

    private IReadOnlyCollection<IgnoreOptionId> GetSelectedIgnoreOptionIds()
    {
        // Полный аналог WinForms: если список пуст — возвращаем кеш, а не пустой набор
        if (_ignoreOptions.Count == 0 || _viewModel.IgnoreOptions.Count == 0)
            return _ignoreSelectionCache;

        var selected = _viewModel.IgnoreOptions
            .Where(o => o.IsChecked)
            .Select(o => o.Id)
            .ToHashSet();

        _ignoreSelectionCache = selected;
        return selected;
    }

    private IgnoreRules BuildIgnoreRules(string rootPath)
    {
        var selected = GetSelectedIgnoreOptionIds();
        return _ignoreRulesService.Build(rootPath, selected);
    }

    private async Task SetClipboardTextAsync(string content)
    {
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;

        if (clipboard != null)
            await clipboard.SetTextAsync(content);
    }

    private bool EnsureTreeReady() => _currentTree is not null && !string.IsNullOrWhiteSpace(_currentPath);

    private HashSet<string> GetCheckedPaths()
    {
        var selected = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var node in _viewModel.TreeNodes)
            CollectChecked(node, selected);
        return selected;
    }

    private static void CollectChecked(TreeNodeViewModel node, HashSet<string> selected)
    {
        if (node.IsChecked)
            selected.Add(node.FullPath);

        foreach (var child in node.Children)
            CollectChecked(child, selected);
    }

    private void UpdateExtensionsSelectionCache()
    {
        // Полный аналог WinForms: если список пуст — кеш НЕ затираем
        if (_viewModel.Extensions.Count == 0)
            return;

        _extensionsSelectionCache = new HashSet<string>(
            _viewModel.Extensions.Where(o => o.IsChecked).Select(o => o.Name),
            StringComparer.OrdinalIgnoreCase);
    }

    private void UpdateIgnoreSelectionCache()
    {
        // Полный аналог WinForms: если список пуст — кеш НЕ затираем
        if (_ignoreOptions.Count == 0 || _viewModel.IgnoreOptions.Count == 0)
            return;

        _ignoreSelectionCache = new HashSet<IgnoreOptionId>(
            _viewModel.IgnoreOptions.Where(o => o.IsChecked).Select(o => o.Id));
    }

    private void SyncIgnoreAllCheckbox()
    {
        SyncAllCheckbox(_viewModel.IgnoreOptions, ref _suppressIgnoreAllCheck, value => _viewModel.AllIgnoreChecked = value);
    }

    private static void SyncAllCheckbox<T>(
        IEnumerable<T> options,
        ref bool suppressFlag,
        Action<bool> setValue)
        where T : class
    {
        suppressFlag = true;
        try
        {
            var list = options.ToList();
            bool allChecked = list.Count > 0 && list.All(option => option switch
            {
                SelectionOptionViewModel selection => selection.IsChecked,
                IgnoreOptionViewModel ignore => ignore.IsChecked,
                _ => false
            });
            setValue(allChecked);
        }
        finally
        {
            suppressFlag = false;
        }
    }

    private static void SetAllChecked<T>(
        IEnumerable<T> options,
        bool isChecked,
        ref bool suppressFlag)
        where T : class
    {
        suppressFlag = true;
        try
        {
            foreach (var option in options)
            {
                switch (option)
                {
                    case SelectionOptionViewModel selection:
                        selection.IsChecked = isChecked;
                        break;
                    case IgnoreOptionViewModel ignore:
                        ignore.IsChecked = isChecked;
                        break;
                }
            }
        }
        finally
        {
            suppressFlag = false;
        }
    }
}
