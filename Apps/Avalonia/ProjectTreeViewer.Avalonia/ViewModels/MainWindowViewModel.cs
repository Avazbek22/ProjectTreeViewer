using System.Collections.ObjectModel;
using ProjectTreeViewer.Application.Services;

namespace ProjectTreeViewer.Avalonia.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    private readonly LocalizationService _localization;

    private string _title;
    private bool _isProjectLoaded;
    private bool _settingsVisible;
    private bool _searchVisible;
    private string _searchQuery = string.Empty;

    private string? _selectedFontFamily;
    private string? _pendingFontFamily;

    private double _treeFontSize = 9;

    private bool _allExtensionsChecked;
    private bool _allRootFoldersChecked;
    private bool _allIgnoreChecked;
    private bool _isDarkTheme;

    public MainWindowViewModel(LocalizationService localization)
    {
        _localization = localization;
        _title = localization["Title.Default"];
        UpdateLocalization();
    }

    public ObservableCollection<TreeNodeViewModel> TreeNodes { get; } = new();
    public ObservableCollection<SelectionOptionViewModel> RootFolders { get; } = new();
    public ObservableCollection<SelectionOptionViewModel> Extensions { get; } = new();
    public ObservableCollection<IgnoreOptionViewModel> IgnoreOptions { get; } = new();
    public ObservableCollection<string> FontFamilies { get; } = new();

    public string Title
    {
        get => _title;
        set
        {
            if (_title == value) return;
            _title = value;
            RaisePropertyChanged();
        }
    }

    public bool IsProjectLoaded
    {
        get => _isProjectLoaded;
        set
        {
            if (_isProjectLoaded == value) return;
            _isProjectLoaded = value;
            RaisePropertyChanged();
        }
    }

    public bool SettingsVisible
    {
        get => _settingsVisible;
        set
        {
            if (_settingsVisible == value) return;
            _settingsVisible = value;
            RaisePropertyChanged();
        }
    }

    public bool SearchVisible
    {
        get => _searchVisible;
        set
        {
            if (_searchVisible == value) return;
            _searchVisible = value;
            RaisePropertyChanged();
        }
    }

    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            if (_searchQuery == value) return;
            _searchQuery = value;
            RaisePropertyChanged();
        }
    }

    public bool IsDarkTheme
    {
        get => _isDarkTheme;
        set
        {
            if (_isDarkTheme == value) return;
            _isDarkTheme = value;
            RaisePropertyChanged();
        }
    }

    // Применённый шрифт (TreeView берет отсюда)
    public string? SelectedFontFamily
    {
        get => _selectedFontFamily;
        set
        {
            if (_selectedFontFamily == value) return;
            _selectedFontFamily = value;
            RaisePropertyChanged();
        }
    }

    // Выбранный в ComboBox (как WinForms _pendingFontName)
    public string? PendingFontFamily
    {
        get => _pendingFontFamily;
        set
        {
            if (_pendingFontFamily == value) return;
            _pendingFontFamily = value;
            RaisePropertyChanged();
        }
    }

    public double TreeFontSize
    {
        get => _treeFontSize;
        set
        {
            if (Math.Abs(_treeFontSize - value) < 0.1) return;
            _treeFontSize = value;
            RaisePropertyChanged();
        }
    }

    public bool AllExtensionsChecked
    {
        get => _allExtensionsChecked;
        set
        {
            if (_allExtensionsChecked == value) return;
            _allExtensionsChecked = value;
            RaisePropertyChanged();
        }
    }

    public bool AllRootFoldersChecked
    {
        get => _allRootFoldersChecked;
        set
        {
            if (_allRootFoldersChecked == value) return;
            _allRootFoldersChecked = value;
            RaisePropertyChanged();
        }
    }

    public bool AllIgnoreChecked
    {
        get => _allIgnoreChecked;
        set
        {
            if (_allIgnoreChecked == value) return;
            _allIgnoreChecked = value;
            RaisePropertyChanged();
        }
    }

    public string MenuFile { get; private set; } = string.Empty;
    public string MenuFileOpen { get; private set; } = string.Empty;
    public string MenuFileRefresh { get; private set; } = string.Empty;
    public string MenuFileExit { get; private set; } = string.Empty;
    public string MenuCopy { get; private set; } = string.Empty;
    public string MenuCopyFullTree { get; private set; } = string.Empty;
    public string MenuCopySelectedTree { get; private set; } = string.Empty;
    public string MenuCopySelectedContent { get; private set; } = string.Empty;
    public string MenuCopyTreeAndContent { get; private set; } = string.Empty;
    public string MenuView { get; private set; } = string.Empty;
    public string MenuViewExpandAll { get; private set; } = string.Empty;
    public string MenuViewCollapseAll { get; private set; } = string.Empty;
    public string MenuViewZoomIn { get; private set; } = string.Empty;
    public string MenuViewZoomOut { get; private set; } = string.Empty;
    public string MenuViewZoomReset { get; private set; } = string.Empty;
    public string MenuViewTheme { get; private set; } = string.Empty;
    public string MenuOptions { get; private set; } = string.Empty;
    public string MenuOptionsTreeSettings { get; private set; } = string.Empty;
    public string MenuLanguage { get; private set; } = string.Empty;
    public string MenuHelp { get; private set; } = string.Empty;
    public string MenuHelpAbout { get; private set; } = string.Empty;
    public string SettingsIgnoreTitle { get; private set; } = string.Empty;
    public string SettingsAll { get; private set; } = string.Empty;
    public string SettingsExtensions { get; private set; } = string.Empty;
    public string SettingsRootFolders { get; private set; } = string.Empty;
    public string SettingsFont { get; private set; } = string.Empty;
    public string SettingsApply { get; private set; } = string.Empty;
    public string MenuSearch { get; private set; } = string.Empty;

    public void UpdateLocalization()
    {
        MenuFile = _localization["Menu.File"];
        MenuFileOpen = _localization["Menu.File.Open"];
        MenuFileRefresh = _localization["Menu.File.Refresh"];
        MenuFileExit = _localization["Menu.File.Exit"];
        MenuCopy = _localization["Menu.Copy"];
        MenuCopyFullTree = _localization["Menu.Copy.FullTree"];
        MenuCopySelectedTree = _localization["Menu.Copy.SelectedTree"];
        MenuCopySelectedContent = _localization["Menu.Copy.SelectedContent"];
        MenuCopyTreeAndContent = _localization["Menu.Copy.FullTreeAndContent"];
        MenuView = _localization["Menu.View"];
        MenuViewExpandAll = _localization["Menu.View.ExpandAll"];
        MenuViewCollapseAll = _localization["Menu.View.CollapseAll"];
        MenuViewZoomIn = _localization["Menu.View.ZoomIn"];
        MenuViewZoomOut = _localization["Menu.View.ZoomOut"];
        MenuViewZoomReset = _localization["Menu.View.ZoomReset"];
        MenuViewTheme = _localization["Menu.View.DarkTheme"];
        MenuOptions = _localization["Menu.Options"];
        MenuOptionsTreeSettings = _localization["Menu.Options.TreeSettings"];
        MenuLanguage = _localization["Menu.Language"];
        MenuHelp = _localization["Menu.Help"];
        MenuHelpAbout = _localization["Menu.Help.About"];
        SettingsIgnoreTitle = _localization["Settings.IgnoreTitle"];
        SettingsAll = _localization["Settings.All"];
        SettingsExtensions = _localization["Settings.Extensions"];
        SettingsRootFolders = _localization["Settings.RootFolders"];
        SettingsFont = _localization["Settings.Font"];
        SettingsApply = _localization["Settings.Apply"];
        MenuSearch = _localization["Menu.Search"];

        RaisePropertyChanged(nameof(MenuFile));
        RaisePropertyChanged(nameof(MenuFileOpen));
        RaisePropertyChanged(nameof(MenuFileRefresh));
        RaisePropertyChanged(nameof(MenuFileExit));
        RaisePropertyChanged(nameof(MenuCopy));
        RaisePropertyChanged(nameof(MenuCopyFullTree));
        RaisePropertyChanged(nameof(MenuCopySelectedTree));
        RaisePropertyChanged(nameof(MenuCopySelectedContent));
        RaisePropertyChanged(nameof(MenuCopyTreeAndContent));
        RaisePropertyChanged(nameof(MenuView));
        RaisePropertyChanged(nameof(MenuViewExpandAll));
        RaisePropertyChanged(nameof(MenuViewCollapseAll));
        RaisePropertyChanged(nameof(MenuViewZoomIn));
        RaisePropertyChanged(nameof(MenuViewZoomOut));
        RaisePropertyChanged(nameof(MenuViewZoomReset));
        RaisePropertyChanged(nameof(MenuViewTheme));
        RaisePropertyChanged(nameof(MenuOptions));
        RaisePropertyChanged(nameof(MenuOptionsTreeSettings));
        RaisePropertyChanged(nameof(MenuLanguage));
        RaisePropertyChanged(nameof(MenuHelp));
        RaisePropertyChanged(nameof(MenuHelpAbout));
        RaisePropertyChanged(nameof(SettingsIgnoreTitle));
        RaisePropertyChanged(nameof(SettingsAll));
        RaisePropertyChanged(nameof(SettingsExtensions));
        RaisePropertyChanged(nameof(SettingsRootFolders));
        RaisePropertyChanged(nameof(SettingsFont));
        RaisePropertyChanged(nameof(SettingsApply));
        RaisePropertyChanged(nameof(MenuSearch));
    }
}
