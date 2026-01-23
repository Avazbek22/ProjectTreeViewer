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
using Avalonia.VisualTree;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
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
    private TextBox? _filterBox;
    private TreeView? _treeView;
    private System.Timers.Timer? _filterDebounceTimer;

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
        _filterBox = this.FindControl<TextBox>("FilterBox");
        _treeView = this.FindControl<TreeView>("ProjectTree");

        _filterDebounceTimer = new System.Timers.Timer(300);
        _filterDebounceTimer.AutoReset = false;
        _filterDebounceTimer.Elapsed += (_, _) =>
        {
            global::Avalonia.Threading.Dispatcher.UIThread.Post(ApplyFilterRealtime);
        };

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
            else if (args.PropertyName == nameof(MainWindowViewModel.NameFilter))
                OnNameFilterChanged();
            else if (args.PropertyName is nameof(MainWindowViewModel.MaterialIntensity)
                     or nameof(MainWindowViewModel.PanelContrast)
                     or nameof(MainWindowViewModel.BorderStrength)
                     or nameof(MainWindowViewModel.MenuChildIntensity))
                UpdateDynamicThemeBrushes();
            else if (args.PropertyName == nameof(MainWindowViewModel.BlurRadius))
                UpdateTransparencyEffect();
        };

        AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);

        Opened += OnOpened;

        // Hook menu item submenu opening to apply brushes directly
        AddHandler(MenuItem.SubmenuOpenedEvent, OnSubmenuOpened, RoutingStrategies.Bubble);
    }

    private void OnSubmenuOpened(object? sender, RoutedEventArgs e)
    {
        if (e.Source is not MenuItem menuItem)
            return;


        Dispatcher.UIThread.Post(() =>
        {
// Защита: если элемент уже отцеплен от дерева (закрыли меню/окно) — ничего не делаем.
            if (menuItem.GetVisualRoot() is null)
                return;


            ApplyBrushesToMenuItemPopup(menuItem);


// Вложенные меню: применяем, но фактически отработает только для IsOpen попапов (см. ниже).
            foreach (var child in menuItem.GetVisualDescendants().OfType<MenuItem>())
            {
                ApplyBrushesToMenuItemPopup(child);
            }
        }, DispatcherPriority.Loaded);
    }



    private void ApplyBrushesToMenuItemPopup(MenuItem menuItem)
    {
        var isChildMenu = menuItem.Parent is MenuItem;


// ВАЖНО: трогаем только ОТКРЫТЫЕ Popup. Закрытые/ещё не созданные попапы приводят к гонкам/крашам.
        foreach (var popup in menuItem.GetVisualDescendants().OfType<Popup>().Where(p => p.IsOpen))
        {
// Принудительное размытие для попап-хоста, чтобы текст дерева "сзади" не мешал.
            ApplyPopupHostEffect(popup);


            if (popup.Child is not Border border)
                continue;


            border.Background = isChildMenu ? _currentMenuChildBrush : _currentMenuBrush;
            border.BorderBrush = _currentBorderBrush;
            border.BorderThickness = new Thickness(1);
            border.CornerRadius = new CornerRadius(8);
            border.Padding = new Thickness(4);
        }
    }

    private void ApplyPopupHostEffect(Popup popup)
    {
// Ключевая защита: если попап уже закрылся — не трогаем ничего.
        if (!popup.IsOpen)
            return;


// Попап создаёт отдельный TopLevel (PopupRoot). Достаём его через Child.
        if (popup.Child is null)
            return;


// Если child ещё не прикреплён к визуальному дереву — рано.
        if (popup.Child.GetVisualRoot() is null)
            return;


        if (TopLevel.GetTopLevel(popup.Child) is not TopLevel topLevel)
            return;


// На всякий случай: не лезем в главное окно (должны менять именно PopupRoot).
        if (ReferenceEquals(topLevel, this))
            return;


// В Avalonia возможна гонка: между Post(...) и выполнением popup успевает уничтожиться.
// Поэтому любые установки свойств TopLevel делаем безопасно.
        try
        {
            if (_viewModel.HasAnyEffect)
            {
                topLevel.TransparencyLevelHint = new[]
                {
                    WindowTransparencyLevel.AcrylicBlur,
                    WindowTransparencyLevel.Blur,
                    WindowTransparencyLevel.Transparent,
                    WindowTransparencyLevel.None
                };


                topLevel.Background = Brushes.Transparent;
            }
            else
            {
                topLevel.TransparencyLevelHint = new[]
                {
                    WindowTransparencyLevel.None
                };
            }
        }
        catch
        {
// Игнорируем: попап мог закрыться/уничтожиться прямо в этот момент.
        }
    }

    private void OnThemeChanged(object? sender, EventArgs e)
    {
        // Defer update to let theme resources settle first
        global::Avalonia.Threading.Dispatcher.UIThread.Post(
            () => UpdateSearchHighlights(_viewModel.SearchQuery),
            global::Avalonia.Threading.DispatcherPriority.Background);
    }

    private async void OnOpened(object? sender, EventArgs e)
    {
        // Set default to Dark theme + Transparent mode
        SetDefaultTheme();

        if (!string.IsNullOrWhiteSpace(_startupOptions.Path))
            TryOpenFolder(_startupOptions.Path!, fromDialog: false);
    }

    private void SetDefaultTheme()
    {
        var app = global::Avalonia.Application.Current;
        if (app is null) return;

        // Set dark theme by default
        app.RequestedThemeVariant = ThemeVariant.Dark;
        _viewModel.IsDarkTheme = true;

        // Set transparent mode by default (already set in ViewModel, ensure transparency hint)
        UpdateTransparencyEffect();
    }

    private void InitializeFonts()
    {
        // Only use predefined fonts like WinForms
        var predefinedFonts = new[]
            { "Consolas", "Courier New", "Fira Code", "Lucida Console", "Cascadia Code", "JetBrains Mono" };

        var systemFonts =
            FontManager.Current?.SystemFonts?.Select(f => f.Name).Distinct(StringComparer.OrdinalIgnoreCase)
                .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Add only predefined fonts that exist on system
        foreach (var font in predefinedFonts)
        {
            if (systemFonts.Contains(font))
                _viewModel.FontFamilies.Add(font);
        }

        // If no predefined fonts found, add Consolas and Courier New as fallbacks
        if (_viewModel.FontFamilies.Count == 0)
        {
            _viewModel.FontFamilies.Add("Consolas");
            _viewModel.FontFamilies.Add("Courier New");
        }

        var selected = _viewModel.FontFamilies.FirstOrDefault() ?? "Consolas";
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

    private void OnZoomReset(object? sender, RoutedEventArgs e) => _viewModel.TreeFontSize = 12;

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

    private void OnSetLightTheme(object? sender, RoutedEventArgs e)
    {
        var app = global::Avalonia.Application.Current;
        if (app is null) return;

        app.RequestedThemeVariant = ThemeVariant.Light;
        _viewModel.IsDarkTheme = false;
        UpdateSearchHighlights(_viewModel.SearchQuery);
        UpdateFilterHighlights(_viewModel.NameFilter);
        UpdateDynamicThemeBrushes();
    }

    private void OnSetDarkTheme(object? sender, RoutedEventArgs e)
    {
        var app = global::Avalonia.Application.Current;
        if (app is null) return;

        app.RequestedThemeVariant = ThemeVariant.Dark;
        _viewModel.IsDarkTheme = true;
        UpdateSearchHighlights(_viewModel.SearchQuery);
        UpdateFilterHighlights(_viewModel.NameFilter);
        UpdateDynamicThemeBrushes();
    }

    private void OnToggleMica(object? sender, RoutedEventArgs e)
    {
        _viewModel.IsMicaEnabled = !_viewModel.IsMicaEnabled;
        UpdateTransparencyEffect();
    }

    private void OnToggleAcrylic(object? sender, RoutedEventArgs e)
    {
        _viewModel.IsAcrylicEnabled = !_viewModel.IsAcrylicEnabled;
        UpdateTransparencyEffect();
    }

    // Current menu brush for direct application
    private SolidColorBrush _currentMenuBrush = new SolidColorBrush(Colors.Black);
    private SolidColorBrush _currentMenuChildBrush = new SolidColorBrush(Colors.Black);
    private SolidColorBrush _currentBorderBrush = new SolidColorBrush(Colors.Gray);

    private void UpdateTransparencyEffect()
    {
        if (!_viewModel.HasAnyEffect)
        {
            TransparencyLevelHint = new[]
            {
                WindowTransparencyLevel.None
            };


            UpdateDynamicThemeBrushes();
            return;
        }


        // Mica
        if (_viewModel.IsMicaEnabled)
        {
            TransparencyLevelHint = new[]
            {
                WindowTransparencyLevel.Mica,
                WindowTransparencyLevel.Blur,
                WindowTransparencyLevel.None
            };


            UpdateDynamicThemeBrushes();
            return;
        }


        // Acrylic
        if (_viewModel.IsAcrylicEnabled)
        {
            TransparencyLevelHint = new[]
            {
                WindowTransparencyLevel.AcrylicBlur,
                WindowTransparencyLevel.Blur,
                WindowTransparencyLevel.Transparent,
                WindowTransparencyLevel.None
            };


            UpdateDynamicThemeBrushes();
            return;
        }


        // Transparent mode
        // В Avalonia нельзя реально менять радиус размытия окна. Поэтому делаем плавность так:
        // - BlurRadius == 0 -> чистая прозрачность (без blur)
        // - BlurRadius > 0 -> включаем AcrylicBlur (есть размытие) и "проявляем" его кистями.
        var blur = Math.Clamp(_viewModel.BlurRadius / 100.0, 0.0, 1.0);


        if (blur <= 0.0001)
        {
            TransparencyLevelHint = new[]
            {
                WindowTransparencyLevel.Transparent,
                WindowTransparencyLevel.None
            };
        }
        else
        {
            TransparencyLevelHint = new[]
            {
                WindowTransparencyLevel.AcrylicBlur,
                WindowTransparencyLevel.Blur,
                WindowTransparencyLevel.Transparent,
                WindowTransparencyLevel.None
            };
        }


        UpdateDynamicThemeBrushes();
    }

    private void UpdateDynamicThemeBrushes()
    {
        if (global::Avalonia.Application.Current is not { } app)
            return;


        var theme = app.ActualThemeVariant ?? ThemeVariant.Dark;
        var isDark = theme == ThemeVariant.Dark;


        // Базовые цвета из Ваших ThemeDictionaries (важно: без эффектов должен быть именно этот фон).
        var baseBg = isDark ? Color.Parse("#121214") : Color.Parse("#FFFFFF");
        var basePanel = isDark ? Color.Parse("#17171A") : Color.Parse("#F3F3F3");


        var material = Math.Clamp(_viewModel.MaterialIntensity / 100.0, 0.0, 1.0);
        var contrast = Math.Clamp(_viewModel.PanelContrast / 100.0, 0.0, 1.0);
        var borderStrength = Math.Clamp(_viewModel.BorderStrength / 100.0, 0.0, 1.0);
        var menuChild = Math.Clamp(_viewModel.MenuChildIntensity / 100.0, 0.0, 1.0);
        var blur = Math.Clamp(_viewModel.BlurRadius / 100.0, 0.0, 1.0);


        // Итоговые значения
        Color bgBase = baseBg;
        Color panelBase = basePanel;


        byte bgAlpha;
        byte panelAlpha;
        byte borderAlpha;
        byte menuAlpha;
        byte menuChildAlpha = 255;
        if (!_viewModel.HasAnyEffect)
        {
            // Без эффектов: полностью непрозрачно
            bgAlpha = 255;
            panelAlpha = 255;
            menuAlpha = 255;
            menuChildAlpha = 255;
        }
        else if (_viewModel.IsMicaEnabled)
        {
            // Mica: усиливаем воспринимаемую "силу" (попросили +500%) через кривую.
            // 0 остаётся 0, но середина/верх становятся заметно сильнее.
            var micaOpen = Math.Pow(material, 0.2); // ~x5 по субъективной силе


            bgAlpha = (byte)Math.Round(255 * (1.0 - micaOpen));


            // Панели держим более читаемыми
            var panelBaseAlpha = 110 + (contrast * 120); // 110..230
            panelAlpha = (byte)Math.Clamp(panelBaseAlpha - (micaOpen * 50), 80, 255);


            // Меню делаем плотнее (плюс размываем попап-хостом)
            menuAlpha = (byte)Math.Clamp(panelAlpha + 35, 160, 255);
            menuChildAlpha = (byte)Math.Clamp(menuAlpha - (menuChild * 40), 140, 255);


            // Чуть более "чёрный" тон в Mica, чтобы не уходить в серо-синий
            if (isDark)
            {
                bgBase = Color.Parse("#101012");
                panelBase = Color.Parse("#151518");
            }
        }
        else if (_viewModel.IsAcrylicEnabled)
        {
            // Acrylic
            // material: 0 -> почти непрозрачно, 1 -> максимально прозрачно
            bgAlpha = (byte)Math.Round(240 - (material * 200)); // 240..40
            panelAlpha = (byte)Math.Round(235 - (material * 150)); // 235..85


            // contrast усиливает панель (делает менее прозрачной)
            panelAlpha = (byte)Math.Clamp(panelAlpha + (contrast * 40), 70, 255);


            menuAlpha = (byte)Math.Clamp(panelAlpha + 30, 150, 255);
            menuChildAlpha = (byte)Math.Clamp(menuAlpha - (menuChild * 40), 130, 255);
        }
        else
        {
            // Transparent
            // material отвечает за прозрачность окна/панелей.
            bgAlpha = (byte)Math.Round(255 * (1.0 - material));


            // "плавность" blur: blur==0 -> прозрачность без blur;
            // blur>0 -> включён AcrylicBlur, и мы мягко повышаем читаемость через панели/меню.
            // (реальный радиус blur у окна в Avalonia не параметризуется)
            var blurVisibility = Math.Pow(blur, 2.2); // 1% почти незаметно, 100% максимально


            var panelBaseAlpha = 90 + (contrast * 130); // 90..220
            panelAlpha = (byte)Math.Clamp(panelBaseAlpha + (blurVisibility * 25), 70, 255);


            // Меню всегда более плотное + попапы будут принудительно с blur-хостом
            menuAlpha = (byte)Math.Clamp(panelAlpha + 45, 170, 255);
            menuChildAlpha = (byte)Math.Clamp(menuAlpha - (menuChild * 40), 150, 255);
        }


        borderAlpha = (byte)Math.Round(255 * borderStrength);


        // Background
        var bgColor = Color.FromArgb(bgAlpha, bgBase.R, bgBase.G, bgBase.B);
        var backgroundBrush = new SolidColorBrush(bgColor);
        UpdateResource("AppBackgroundBrush", backgroundBrush);

        // Panel
        var panelColor = Color.FromArgb(panelAlpha, panelBase.R, panelBase.G, panelBase.B);
        var panelBrush = new SolidColorBrush(panelColor);
        UpdateResource("AppPanelBrush", panelBrush);

        // Menu popup (top-level)
        var menuColor = Color.FromArgb(menuAlpha, panelBase.R, panelBase.G, panelBase.B);
        _currentMenuBrush = new SolidColorBrush(menuColor);
        UpdateResource("MenuPopupBrush", _currentMenuBrush);

        // Menu popup (child/submenu)
        var menuChildColor = Color.FromArgb(menuChildAlpha, panelBase.R, panelBase.G, panelBase.B);
        _currentMenuChildBrush = new SolidColorBrush(menuChildColor);
        UpdateResource("MenuChildPopupBrush", _currentMenuChildBrush);

        // Border
        var borderBase = isDark ? Color.Parse("#505050") : Color.Parse("#C0C0C0");
        var borderColor = Color.FromArgb(borderAlpha, borderBase.R, borderBase.G, borderBase.B);
        _currentBorderBrush = new SolidColorBrush(borderColor);
        UpdateResource("AppBorderBrush", _currentBorderBrush);

        // Accent (если нужно — оставьте как было)
        var accentColor = isDark ? Color.Parse("#4CC2FF") : Color.Parse("#0078D4");
        UpdateResource("AppAccentBrush", new SolidColorBrush(accentColor));

        // Принудительно подтолкнуть открытые попапы меню
        ApplyMenuBrushesDirect();
    }

    private void UpdateResourceForCurrentTheme(global::Avalonia.Application app, ThemeVariant theme, string key,
        object value)
    {
// 1) Локально в окне (самый высокий приоритет)
        Resources[key] = value;


// 2) В Application (на всякий случай, если где-то берут напрямую оттуда)
        app.Resources[key] = value;


// 3) И в ThemeDictionaries (если ресурс там определён и перекрывает lookup)
        if (app.Resources.ThemeDictionaries is { } dict && dict.TryGetValue(theme, out var themed) &&
            themed is ResourceDictionary rd)
        {
            rd[key] = value;
        }
    }

    private void ApplyMenuBrushesDirect()
    {
        // Find all MenuItems and hook their popup opening
        var mainMenu = this.FindControl<Menu>("MainMenu");
        if (mainMenu is null) return;

        foreach (var menuItem in mainMenu.GetLogicalDescendants().OfType<MenuItem>())
        {
            // Update any currently open popup
            UpdateMenuItemPopup(menuItem);
        }
    }

    private void UpdateMenuItemPopup(MenuItem menuItem)
    {
        // Check if this is a child menu (has a parent MenuItem) or top-level menu
        var isChildMenu = menuItem.Parent is MenuItem;

        // Find the popup template part
        var popup = menuItem.GetVisualDescendants().OfType<Popup>().FirstOrDefault();
        if (popup?.Child is Border border)
        {
            border.Background = isChildMenu ? _currentMenuChildBrush : _currentMenuBrush;
            border.BorderBrush = _currentBorderBrush;
        }

        // Also check logical descendants for nested items - these are always child menus
        foreach (var subItem in menuItem.GetLogicalDescendants().OfType<MenuItem>())
        {
            var subPopup = subItem.GetVisualDescendants().OfType<Popup>().FirstOrDefault();
            if (subPopup?.Child is Border subBorder)
            {
                subBorder.Background = _currentMenuChildBrush;
                subBorder.BorderBrush = _currentBorderBrush;
            }
        }
    }

    private void UpdateResource(string key, object value)
    {
        var app = global::Avalonia.Application.Current;

        // Update in Application resources
        if (app?.Resources is not null)
        {
            try
            {
                app.Resources[key] = value;
            }
            catch
            {
                // Ignore errors
            }
        }

        // Also update in Window resources for immediate effect on all elements
        try
        {
            Resources[key] = value;
        }
        catch
        {
            // Ignore errors
        }
    }

    private void OnToggleCompactMode(object? sender, RoutedEventArgs e)
    {
        _viewModel.IsCompactMode = !_viewModel.IsCompactMode;

        if (_viewModel.IsCompactMode)
            Classes.Add("compact-mode");
        else
            Classes.Remove("compact-mode");
    }

    private void OnThemeMenuClick(object? sender, RoutedEventArgs e)
    {
        _viewModel.ThemePopoverOpen = !_viewModel.ThemePopoverOpen;
        e.Handled = true;
    }

    private void OnSetLightThemeCheckbox(object? sender, RoutedEventArgs e)
    {
        // Always set light theme when clicked (even if already light - just refresh)
        OnSetLightTheme(sender, e);
        e.Handled = true;
    }

    private void OnSetDarkThemeCheckbox(object? sender, RoutedEventArgs e)
    {
        // Always set dark theme when clicked
        OnSetDarkTheme(sender, e);
        e.Handled = true;
    }

    private void OnSetTransparentMode(object? sender, RoutedEventArgs e)
    {
        _viewModel.ToggleTransparent();
        UpdateTransparencyEffect();
        e.Handled = true;
    }

    private void OnSetMicaMode(object? sender, RoutedEventArgs e)
    {
        _viewModel.ToggleMica();
        UpdateTransparencyEffect();
        e.Handled = true;
    }

    private void OnSetAcrylicMode(object? sender, RoutedEventArgs e)
    {
        _viewModel.ToggleAcrylic();
        UpdateTransparencyEffect();
        e.Handled = true;
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

    private void OnToggleFilter(object? sender, RoutedEventArgs e)
    {
        if (!_viewModel.IsProjectLoaded) return;

        if (_viewModel.FilterVisible)
        {
            CloseFilter();
            return;
        }

        ShowFilter();
    }

    private void OnFilterClose(object? sender, RoutedEventArgs e) => CloseFilter();

    private void OnFilterKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            CloseFilter();
            e.Handled = true;
        }
    }

    private void ShowFilter()
    {
        if (!_viewModel.IsProjectLoaded) return;

        _viewModel.FilterVisible = true;
        _filterBox?.Focus();
        _filterBox?.SelectAll();
    }

    private void CloseFilter()
    {
        if (!_viewModel.FilterVisible) return;

        _viewModel.FilterVisible = false;
        _viewModel.NameFilter = string.Empty;
        ApplyFilterRealtime();
        _treeView?.Focus();
    }

    private void OnNameFilterChanged()
    {
        _filterDebounceTimer?.Stop();
        _filterDebounceTimer?.Start();
    }

    private void ApplyFilterRealtime()
    {
        if (string.IsNullOrEmpty(_currentPath)) return;

        RefreshTree();
        UpdateFilterHighlights(_viewModel.NameFilter);

        // Auto-expand folders with matching items
        if (!string.IsNullOrWhiteSpace(_viewModel.NameFilter))
        {
            SmartExpandForFilter(_viewModel.NameFilter);
        }
    }

    private void UpdateFilterHighlights(string? query)
    {
        var (highlightBackground, highlightForeground, normalForeground) = GetSearchHighlightBrushes();
        foreach (var node in _viewModel.TreeNodes.SelectMany(n => n.Flatten()))
            node.UpdateSearchHighlight(query, highlightBackground, highlightForeground, normalForeground);
    }

    private void SmartExpandForFilter(string filter)
    {
        foreach (var node in _viewModel.TreeNodes)
        {
            SmartExpandNode(node, filter);
        }
    }

    private bool SmartExpandNode(TreeNodeViewModel node, string filter)
    {
        bool hasMatchingDescendant = false;
        bool selfMatches = node.DisplayName.Contains(filter, StringComparison.OrdinalIgnoreCase);

        foreach (var child in node.Children)
        {
            if (SmartExpandNode(child, filter))
                hasMatchingDescendant = true;
        }

        // Expand this node if it has matching children (but not if only self matches)
        if (hasMatchingDescendant)
            node.IsExpanded = true;
        else if (!selfMatches)
            node.IsExpanded = false;

        return selfMatches || hasMatchingDescendant;
    }

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

        // Ctrl+Shift+N - Filter by name
        if (mods == (KeyModifiers.Control | KeyModifiers.Shift) && e.Key == Key.N)
        {
            if (_viewModel.IsProjectLoaded)
                OnToggleFilter(this, new RoutedEventArgs());
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
            // Collapse all when search is cleared
            foreach (var node in _viewModel.TreeNodes)
            {
                CollapseAllExceptRoot(node);
            }

            return;
        }

        foreach (var node in _viewModel.TreeNodes.SelectMany(n => n.Flatten()))
        {
            if (node.DisplayName.Contains(query, StringComparison.OrdinalIgnoreCase))
                _searchMatches.Add(node);
        }

        // Smart expand - only expand folders that contain matching items
        foreach (var node in _viewModel.TreeNodes)
        {
            SmartExpandForSearch(node, query);
        }

        if (_searchMatches.Count > 0)
        {
            _searchMatchIndex = 0;
            SelectSearchMatch();
        }
    }

    private bool SmartExpandForSearch(TreeNodeViewModel node, string query)
    {
        bool hasMatchingDescendant = false;
        bool selfMatches = node.DisplayName.Contains(query, StringComparison.OrdinalIgnoreCase);

        foreach (var child in node.Children)
        {
            if (SmartExpandForSearch(child, query))
                hasMatchingDescendant = true;
        }

        // Expand this node if it has matching children
        if (hasMatchingDescendant)
            node.IsExpanded = true;
        else if (!selfMatches && node.Children.Count > 0)
            node.IsExpanded = false;

        return selfMatches || hasMatchingDescendant;
    }

    private void CollapseAllExceptRoot(TreeNodeViewModel node)
    {
        foreach (var child in node.Children)
        {
            child.IsExpanded = false;
            CollapseAllExceptRoot(child);
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
        var (highlightBackground, highlightForeground, normalForeground) = GetSearchHighlightBrushes();
        foreach (var node in _viewModel.TreeNodes.SelectMany(n => n.Flatten()))
            node.UpdateSearchHighlight(query, highlightBackground, highlightForeground, normalForeground);
    }

    private (IBrush highlightBackground, IBrush highlightForeground, IBrush normalForeground)
        GetSearchHighlightBrushes()
    {
        var app = global::Avalonia.Application.Current;
        var theme = app?.ActualThemeVariant ?? ThemeVariant.Light;

        // Set fallback defaults FIRST based on theme
        IBrush highlightBackground = new SolidColorBrush(Color.Parse("#FFEB3B"));
        IBrush highlightForeground = new SolidColorBrush(Color.Parse("#000000"));
        IBrush normalForeground = theme == ThemeVariant.Dark
            ? new SolidColorBrush(Color.Parse("#E7E9EF"))
            : new SolidColorBrush(Color.Parse("#1A1A1A"));

        // Override with resources only if they're valid IBrush (pattern matching)
        if (app?.Resources.TryGetResource("TreeSearchHighlightBrush", theme, out var bg) == true &&
            bg is IBrush bgBrush)
            highlightBackground = bgBrush;

        if (app?.Resources.TryGetResource("TreeSearchHighlightTextBrush", theme, out var fg) == true &&
            fg is IBrush fgBrush)
            highlightForeground = fgBrush;

        if (app?.Resources.TryGetResource("AppTextBrush", theme, out var textFg) == true && textFg is IBrush textBrush)
            normalForeground = textBrush;

        return (highlightBackground, highlightForeground, normalForeground);
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

        // Get value directly from control - event fires BEFORE binding updates ViewModel
        var check = (sender as CheckBox)?.IsChecked == true;
        _suppressRootAllCheck = true;
        _viewModel.AllRootFoldersChecked = check;
        _suppressRootAllCheck = false;

        SetAllChecked(_viewModel.RootFolders, check, ref _suppressRootItemCheck);
        UpdateLiveOptionsFromRootSelection();
    }

    private void OnExtensionsAllChanged(object? sender, RoutedEventArgs e)
    {
        if (_suppressExtensionAllCheck) return;

        // Get value directly from control - event fires BEFORE binding updates ViewModel
        var check = (sender as CheckBox)?.IsChecked == true;
        _suppressExtensionAllCheck = true;
        _viewModel.AllExtensionsChecked = check;
        _suppressExtensionAllCheck = false;

        SetAllChecked(_viewModel.Extensions, check, ref _suppressExtensionItemCheck);
        UpdateExtensionsSelectionCache();
    }

    private void OnIgnoreAllChanged(object? sender, RoutedEventArgs e)
    {
        if (_suppressIgnoreAllCheck) return;

        _ignoreSelectionInitialized = true;

        // Get value directly from control - event fires BEFORE binding updates ViewModel
        var check = (sender as CheckBox)?.IsChecked == true;
        _suppressIgnoreAllCheck = true;
        _viewModel.AllIgnoreChecked = check;
        _suppressIgnoreAllCheck = false;

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

        var nameFilter = string.IsNullOrWhiteSpace(_viewModel.NameFilter) ? null : _viewModel.NameFilter.Trim();

        var options = new TreeFilterOptions(
            AllowedExtensions: allowedExt,
            AllowedRootFolders: allowedRoot,
            IgnoreRules: ignoreRules,
            NameFilter: nameFilter);

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
            : new HashSet<string>(_viewModel.Extensions.Where(o => o.IsChecked).Select(o => o.Name),
                StringComparer.OrdinalIgnoreCase);

        if (rootFolders.Count == 0)
        {
            _viewModel.Extensions.Clear();
            _suppressExtensionAllCheck = true;
            _viewModel.AllExtensionsChecked = false;
            _suppressExtensionAllCheck = false;
            SyncAllCheckbox(_viewModel.Extensions, ref _suppressExtensionAllCheck,
                value => _viewModel.AllExtensionsChecked = value);
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

        SyncAllCheckbox(_viewModel.Extensions, ref _suppressExtensionAllCheck,
            value => _viewModel.AllExtensionsChecked = value);
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

        SyncAllCheckbox(_viewModel.RootFolders, ref _suppressRootAllCheck,
            value => _viewModel.AllRootFoldersChecked = value);
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
        SyncAllCheckbox(_viewModel.IgnoreOptions, ref _suppressIgnoreAllCheck,
            value => _viewModel.AllIgnoreChecked = value);
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