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
    private string _nameFilter = string.Empty;

    private string? _selectedFontFamily;
    private string? _pendingFontFamily;

    private double _treeFontSize = 12;

    private bool _allExtensionsChecked;
    private bool _allRootFoldersChecked;
    private bool _allIgnoreChecked;
    private bool _isDarkTheme;
    private bool _isCompactMode;
    private bool _filterVisible;
    private bool _isMicaEnabled = true;
    private bool _isAcrylicEnabled;

    public MainWindowViewModel(LocalizationService localization)
    {
        _localization = localization;
        _title = localization["Title.Default"];
        _allExtensionsChecked = true;
        _allRootFoldersChecked = true;
        _allIgnoreChecked = true;
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

    public string NameFilter
    {
        get => _nameFilter;
        set
        {
            if (_nameFilter == value) return;
            _nameFilter = value;
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

    public bool IsCompactMode
    {
        get => _isCompactMode;
        set
        {
            if (_isCompactMode == value) return;
            _isCompactMode = value;
            RaisePropertyChanged();
        }
    }

    public bool FilterVisible
    {
        get => _filterVisible;
        set
        {
            if (_filterVisible == value) return;
            _filterVisible = value;
            RaisePropertyChanged();
        }
    }

    public bool IsMicaEnabled
    {
        get => _isMicaEnabled;
        set
        {
            if (_isMicaEnabled == value) return;
            _isMicaEnabled = value;
            if (value) _isAcrylicEnabled = false;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(IsAcrylicEnabled));
        }
    }

    public bool IsAcrylicEnabled
    {
        get => _isAcrylicEnabled;
        set
        {
            if (_isAcrylicEnabled == value) return;
            _isAcrylicEnabled = value;
            if (value) _isMicaEnabled = false;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(IsMicaEnabled));
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
            RaisePropertyChanged(nameof(TreeIconSize));
        }
    }

    public double TreeIconSize => Math.Max(12, Math.Round(TreeFontSize * 1.25, 0));

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
    public string MenuViewThemeTitle { get; private set; } = string.Empty;
    public string MenuViewLightTheme { get; private set; } = string.Empty;
    public string MenuViewDarkTheme { get; private set; } = string.Empty;
    public string MenuViewMica { get; private set; } = string.Empty;
    public string MenuViewAcrylic { get; private set; } = string.Empty;
    public string MenuViewCompactMode { get; private set; } = string.Empty;
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
    public string FilterByNamePlaceholder { get; private set; } = string.Empty;
    public string FilterTooltip { get; private set; } = string.Empty;

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
        MenuViewThemeTitle = _localization["Menu.View.Theme"];
        MenuViewLightTheme = _localization["Menu.View.LightTheme"];
        MenuViewDarkTheme = _localization["Menu.View.DarkTheme"];
        MenuViewMica = _localization["Menu.View.Mica"];
        MenuViewAcrylic = _localization["Menu.View.Acrylic"];
        MenuViewCompactMode = _localization["Menu.View.CompactMode"];
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
        FilterByNamePlaceholder = _localization["Filter.ByName"];
        FilterTooltip = _localization["Filter.Tooltip"];

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
        RaisePropertyChanged(nameof(MenuViewThemeTitle));
        RaisePropertyChanged(nameof(MenuViewLightTheme));
        RaisePropertyChanged(nameof(MenuViewDarkTheme));
        RaisePropertyChanged(nameof(MenuViewMica));
        RaisePropertyChanged(nameof(MenuViewAcrylic));
        RaisePropertyChanged(nameof(MenuViewCompactMode));
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
        RaisePropertyChanged(nameof(FilterByNamePlaceholder));
        RaisePropertyChanged(nameof(FilterTooltip));
    }
}
