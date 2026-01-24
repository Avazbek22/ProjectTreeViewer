using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace ProjectTreeViewer.Avalonia.Views;

public partial class TopMenuBarView : UserControl
{
    private TopLevel? _helpPopupTopLevel;
    private bool _helpPopupHandlersAttached;
    private TopLevel? _helpDocsPopupTopLevel;
    private bool _helpDocsPopupHandlersAttached;

    public event EventHandler<RoutedEventArgs>? OpenFolderRequested;
    public event EventHandler<RoutedEventArgs>? RefreshRequested;
    public event EventHandler<RoutedEventArgs>? ExitRequested;
    public event EventHandler<RoutedEventArgs>? CopyFullTreeRequested;
    public event EventHandler<RoutedEventArgs>? CopySelectedTreeRequested;
    public event EventHandler<RoutedEventArgs>? CopySelectedContentRequested;
    public event EventHandler<RoutedEventArgs>? CopyTreeAndContentRequested;
    public event EventHandler<RoutedEventArgs>? ExpandAllRequested;
    public event EventHandler<RoutedEventArgs>? CollapseAllRequested;
    public event EventHandler<RoutedEventArgs>? ZoomInRequested;
    public event EventHandler<RoutedEventArgs>? ZoomOutRequested;
    public event EventHandler<RoutedEventArgs>? ZoomResetRequested;
    public event EventHandler<RoutedEventArgs>? ToggleCompactModeRequested;
    public event EventHandler<RoutedEventArgs>? ToggleSearchRequested;
    public event EventHandler<RoutedEventArgs>? ToggleSettingsRequested;
    public event EventHandler<RoutedEventArgs>? ToggleFilterRequested;
    public event EventHandler<RoutedEventArgs>? ThemeMenuClickRequested;
    public event EventHandler<RoutedEventArgs>? LanguageRuRequested;
    public event EventHandler<RoutedEventArgs>? LanguageEnRequested;
    public event EventHandler<RoutedEventArgs>? LanguageUzRequested;
    public event EventHandler<RoutedEventArgs>? LanguageTgRequested;
    public event EventHandler<RoutedEventArgs>? LanguageKkRequested;
    public event EventHandler<RoutedEventArgs>? LanguageFrRequested;
    public event EventHandler<RoutedEventArgs>? LanguageDeRequested;
    public event EventHandler<RoutedEventArgs>? LanguageItRequested;
    public event EventHandler<RoutedEventArgs>? HelpRequested;
    public event EventHandler<RoutedEventArgs>? HelpCloseRequested;
    public event EventHandler<RoutedEventArgs>? AboutRequested;
    public event EventHandler<RoutedEventArgs>? AboutCloseRequested;
    public event EventHandler<RoutedEventArgs>? AboutOpenLinkRequested;
    public event EventHandler<RoutedEventArgs>? AboutCopyLinkRequested;
    public event EventHandler<RoutedEventArgs>? SetLightThemeRequested;
    public event EventHandler<RoutedEventArgs>? SetDarkThemeRequested;
    public event EventHandler<RoutedEventArgs>? SetTransparentModeRequested;
    public event EventHandler<RoutedEventArgs>? SetMicaModeRequested;
    public event EventHandler<RoutedEventArgs>? SetAcrylicModeRequested;

    public TopMenuBarView()
    {
        InitializeComponent();

        var popover = ThemePopover;
        if (popover is not null)
        {
            popover.SetLightThemeRequested += (_, e) => SetLightThemeRequested?.Invoke(this, e);
            popover.SetDarkThemeRequested += (_, e) => SetDarkThemeRequested?.Invoke(this, e);
            popover.SetTransparentModeRequested += (_, e) => SetTransparentModeRequested?.Invoke(this, e);
            popover.SetMicaModeRequested += (_, e) => SetMicaModeRequested?.Invoke(this, e);
            popover.SetAcrylicModeRequested += (_, e) => SetAcrylicModeRequested?.Invoke(this, e);
        }

        var helpPopover = HelpPopover;
        if (helpPopover is not null)
        {
            helpPopover.CloseRequested += (_, e) => AboutCloseRequested?.Invoke(this, e);
            helpPopover.OpenLinkRequested += (_, e) => AboutOpenLinkRequested?.Invoke(this, e);
            helpPopover.CopyLinkRequested += (_, e) => AboutCopyLinkRequested?.Invoke(this, e);
        }

        var helpPopup = HelpPopup;
        if (helpPopup is not null)
        {
            helpPopup.Opened += OnHelpPopupOpened;
            helpPopup.Closed += OnHelpPopupClosed;
        }

        var helpDocsPopover = HelpDocsPopover;
        if (helpDocsPopover is not null)
            helpDocsPopover.CloseRequested += (_, e) => HelpCloseRequested?.Invoke(this, e);

        var helpDocsPopup = HelpDocsPopup;
        if (helpDocsPopup is not null)
        {
            helpDocsPopup.Opened += OnHelpDocsPopupOpened;
            helpDocsPopup.Closed += OnHelpDocsPopupClosed;
        }
    }

    public Menu? MainMenuControl => MainMenu;

    private void OnOpenFolder(object? sender, RoutedEventArgs e) => OpenFolderRequested?.Invoke(sender, e);

    private void OnRefresh(object? sender, RoutedEventArgs e) => RefreshRequested?.Invoke(sender, e);

    private void OnExit(object? sender, RoutedEventArgs e) => ExitRequested?.Invoke(sender, e);

    private void OnCopyFullTree(object? sender, RoutedEventArgs e) => CopyFullTreeRequested?.Invoke(sender, e);

    private void OnCopySelectedTree(object? sender, RoutedEventArgs e) => CopySelectedTreeRequested?.Invoke(sender, e);

    private void OnCopySelectedContent(object? sender, RoutedEventArgs e)
        => CopySelectedContentRequested?.Invoke(sender, e);

    private void OnCopyTreeAndContent(object? sender, RoutedEventArgs e)
        => CopyTreeAndContentRequested?.Invoke(sender, e);

    private void OnExpandAll(object? sender, RoutedEventArgs e) => ExpandAllRequested?.Invoke(sender, e);

    private void OnCollapseAll(object? sender, RoutedEventArgs e) => CollapseAllRequested?.Invoke(sender, e);

    private void OnZoomIn(object? sender, RoutedEventArgs e) => ZoomInRequested?.Invoke(sender, e);

    private void OnZoomOut(object? sender, RoutedEventArgs e) => ZoomOutRequested?.Invoke(sender, e);

    private void OnZoomReset(object? sender, RoutedEventArgs e) => ZoomResetRequested?.Invoke(sender, e);

    private void OnToggleCompactMode(object? sender, RoutedEventArgs e)
        => ToggleCompactModeRequested?.Invoke(sender, e);

    private void OnToggleSearch(object? sender, RoutedEventArgs e) => ToggleSearchRequested?.Invoke(sender, e);

    private void OnToggleSettings(object? sender, RoutedEventArgs e) => ToggleSettingsRequested?.Invoke(sender, e);

    private void OnToggleFilter(object? sender, RoutedEventArgs e) => ToggleFilterRequested?.Invoke(sender, e);

    private void OnThemeMenuClick(object? sender, RoutedEventArgs e)
        => ThemeMenuClickRequested?.Invoke(sender, e);

    private void OnLangRu(object? sender, RoutedEventArgs e) => LanguageRuRequested?.Invoke(sender, e);

    private void OnLangEn(object? sender, RoutedEventArgs e) => LanguageEnRequested?.Invoke(sender, e);

    private void OnLangUz(object? sender, RoutedEventArgs e) => LanguageUzRequested?.Invoke(sender, e);

    private void OnLangTg(object? sender, RoutedEventArgs e) => LanguageTgRequested?.Invoke(sender, e);

    private void OnLangKk(object? sender, RoutedEventArgs e) => LanguageKkRequested?.Invoke(sender, e);

    private void OnLangFr(object? sender, RoutedEventArgs e) => LanguageFrRequested?.Invoke(sender, e);

    private void OnLangDe(object? sender, RoutedEventArgs e) => LanguageDeRequested?.Invoke(sender, e);

    private void OnLangIt(object? sender, RoutedEventArgs e) => LanguageItRequested?.Invoke(sender, e);

    private void OnHelp(object? sender, RoutedEventArgs e) => HelpRequested?.Invoke(sender, e);

    private void OnAbout(object? sender, RoutedEventArgs e) => AboutRequested?.Invoke(sender, e);

    private void OnHelpPopupOpened(object? sender, EventArgs e)
    {
        HelpPopover?.Focus();
        ClampPopupToWindow(HelpPopup);

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
            return;

        if (_helpPopupHandlersAttached && _helpPopupTopLevel == topLevel)
            return;

        DetachHelpPopupHandlers();
        _helpPopupTopLevel = topLevel;
        topLevel.AddHandler(InputElement.GotFocusEvent, OnTopLevelGotFocus, RoutingStrategies.Tunnel);
        topLevel.AddHandler(InputElement.PointerPressedEvent, OnTopLevelPointerPressed, RoutingStrategies.Tunnel);
        _helpPopupHandlersAttached = true;
    }

    private void OnHelpPopupClosed(object? sender, EventArgs e)
    {
        DetachHelpPopupHandlers();
    }

    private void OnTopLevelGotFocus(object? sender, GotFocusEventArgs e)
    {
        if (HelpPopup?.IsOpen != true)
            return;

        if (!IsInsidePopup(HelpPopup, e.Source as Visual))
            HelpPopup.IsOpen = false;
    }

    private void OnTopLevelPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (HelpPopup?.IsOpen != true)
            return;

        if (!IsInsidePopup(HelpPopup, e.Source as Visual))
            HelpPopup.IsOpen = false;
    }

    private bool IsInsidePopup(Popup? popup, Visual? source)
    {
        var popupRoot = popup?.Child as Visual;
        if (popupRoot is null || source is null)
            return false;

        return popupRoot == source || popupRoot.IsVisualAncestorOf(source);
    }

    private void DetachHelpPopupHandlers()
    {
        if (!_helpPopupHandlersAttached || _helpPopupTopLevel is null)
            return;

        _helpPopupTopLevel.RemoveHandler(InputElement.GotFocusEvent, OnTopLevelGotFocus);
        _helpPopupTopLevel.RemoveHandler(InputElement.PointerPressedEvent, OnTopLevelPointerPressed);
        _helpPopupTopLevel = null;
        _helpPopupHandlersAttached = false;
    }

    private void OnHelpDocsPopupOpened(object? sender, EventArgs e)
    {
        HelpDocsPopover?.Focus();
        ClampPopupToWindow(HelpDocsPopup);

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
            return;

        if (_helpDocsPopupHandlersAttached && _helpDocsPopupTopLevel == topLevel)
            return;

        DetachHelpDocsPopupHandlers();
        _helpDocsPopupTopLevel = topLevel;
        topLevel.AddHandler(InputElement.GotFocusEvent, OnTopLevelHelpDocsGotFocus, RoutingStrategies.Tunnel);
        topLevel.AddHandler(InputElement.PointerPressedEvent, OnTopLevelHelpDocsPointerPressed, RoutingStrategies.Tunnel);
        _helpDocsPopupHandlersAttached = true;
    }

    private void OnHelpDocsPopupClosed(object? sender, EventArgs e)
    {
        DetachHelpDocsPopupHandlers();
    }

    private void OnTopLevelHelpDocsGotFocus(object? sender, GotFocusEventArgs e)
    {
        if (HelpDocsPopup?.IsOpen != true)
            return;

        if (!IsInsidePopup(HelpDocsPopup, e.Source as Visual))
            HelpDocsPopup.IsOpen = false;
    }

    private void OnTopLevelHelpDocsPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (HelpDocsPopup?.IsOpen != true)
            return;

        if (!IsInsidePopup(HelpDocsPopup, e.Source as Visual))
            HelpDocsPopup.IsOpen = false;
    }

    private void DetachHelpDocsPopupHandlers()
    {
        if (!_helpDocsPopupHandlersAttached || _helpDocsPopupTopLevel is null)
            return;

        _helpDocsPopupTopLevel.RemoveHandler(InputElement.GotFocusEvent, OnTopLevelHelpDocsGotFocus);
        _helpDocsPopupTopLevel.RemoveHandler(InputElement.PointerPressedEvent, OnTopLevelHelpDocsPointerPressed);
        _helpDocsPopupTopLevel = null;
        _helpDocsPopupHandlersAttached = false;
    }

    private void ClampPopupToWindow(Popup? popup)
    {
        if (popup?.Child is not Control child)
            return;

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
            return;

        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            var origin = child.TranslatePoint(new Point(0, 0), topLevel);
            if (origin is null)
                return;

            var bounds = topLevel.Bounds;
            var size = child.Bounds.Size;
            const double margin = 8;

            var left = origin.Value.X;
            var top = origin.Value.Y;
            var right = left + size.Width;
            var bottom = top + size.Height;

            var offsetX = 0.0;
            var offsetY = 0.0;

            if (left < margin)
                offsetX = margin - left;
            else if (right > bounds.Width - margin)
                offsetX = bounds.Width - margin - right;

            if (top < margin)
                offsetY = margin - top;
            else if (bottom > bounds.Height - margin)
                offsetY = bounds.Height - margin - bottom;

            if (Math.Abs(offsetX) > 0.1)
                popup.HorizontalOffset += offsetX;
            if (Math.Abs(offsetY) > 0.1)
                popup.VerticalOffset += offsetY;
        }, Avalonia.Threading.DispatcherPriority.Background);
    }
}
