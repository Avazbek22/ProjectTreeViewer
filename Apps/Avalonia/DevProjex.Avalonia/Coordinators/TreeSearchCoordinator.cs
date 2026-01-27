using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Styling;
using DevProjex.Application.Services;
using DevProjex.Avalonia.ViewModels;

namespace DevProjex.Avalonia.Coordinators;

public sealed class TreeSearchCoordinator
{
    private readonly MainWindowViewModel _viewModel;
    private readonly TreeView _treeView;
    private readonly List<TreeNodeViewModel> _searchMatches = new();
    private int _searchMatchIndex = -1;
    private TreeNodeViewModel? _currentSearchMatch;

    public TreeSearchCoordinator(MainWindowViewModel viewModel, TreeView treeView)
    {
        _viewModel = viewModel;
        _treeView = treeView;
    }

    public void UpdateSearchMatches()
    {
        _searchMatches.Clear();
        _searchMatchIndex = -1;
        UpdateCurrentSearchMatch(null);

        var query = _viewModel.SearchQuery;
        UpdateHighlights(query);
        if (string.IsNullOrWhiteSpace(query))
        {
            foreach (var node in _viewModel.TreeNodes)
                CollapseAllExceptRoot(node);
            return;
        }

        _searchMatches.AddRange(TreeSearchEngine.CollectMatches(
            _viewModel.TreeNodes,
            query,
            node => node.DisplayName,
            node => node.Children,
            StringComparison.OrdinalIgnoreCase));

        TreeSearchEngine.ApplySmartExpandForSearch(
            _viewModel.TreeNodes,
            query,
            node => node.DisplayName,
            node => node.Children,
            node => node.Children.Count > 0,
            (node, expanded) => node.IsExpanded = expanded);

        if (_searchMatches.Count > 0)
        {
            _searchMatchIndex = 0;
            SelectSearchMatch();
        }
    }

    public void UpdateHighlights(string? query)
    {
        var (highlightBackground, highlightForeground, normalForeground, currentBackground) = GetSearchHighlightBrushes();
        foreach (var node in _viewModel.TreeNodes.SelectMany(n => n.Flatten()))
            node.UpdateSearchHighlight(query, highlightBackground, highlightForeground, normalForeground, currentBackground);
    }

    public void ClearSearchState()
    {
        _searchMatches.Clear();
        _searchMatches.TrimExcess(); // Release allocated memory
        _searchMatchIndex = -1;
        UpdateCurrentSearchMatch(null);
        UpdateHighlights(string.Empty);
    }

    public void Navigate(int step)
    {
        if (_searchMatches.Count == 0)
            return;

        _searchMatchIndex = (_searchMatchIndex + step + _searchMatches.Count) % _searchMatches.Count;
        SelectSearchMatch();
    }

    public void RefreshThemeHighlights()
    {
        UpdateHighlights(_viewModel.SearchQuery);
    }

    private void SelectSearchMatch()
    {
        if (_searchMatchIndex < 0 || _searchMatchIndex >= _searchMatches.Count)
            return;

        var node = _searchMatches[_searchMatchIndex];
        node.EnsureParentsExpanded();
        SelectTreeNode(node);
        UpdateCurrentSearchMatch(node);
        BringNodeIntoView(node);
        _treeView.Focus();
    }

    private void BringNodeIntoView(TreeNodeViewModel node)
    {
        var item = _treeView.GetLogicalDescendants()
            .OfType<TreeViewItem>()
            .FirstOrDefault(container => ReferenceEquals(container.DataContext, node));

        item?.BringIntoView();
    }

    private void SelectTreeNode(TreeNodeViewModel node)
    {
        _treeView.SelectedItem = node;
        node.IsSelected = true;
    }

    private void UpdateCurrentSearchMatch(TreeNodeViewModel? node)
    {
        if (ReferenceEquals(_currentSearchMatch, node))
            return;

        var query = _viewModel.SearchQuery;
        var (highlightBackground, highlightForeground, normalForeground, currentBackground) = GetSearchHighlightBrushes();

        if (_currentSearchMatch is not null)
        {
            _currentSearchMatch.IsCurrentSearchMatch = false;
            _currentSearchMatch.UpdateSearchHighlight(
                query,
                highlightBackground,
                highlightForeground,
                normalForeground,
                currentBackground);
        }

        _currentSearchMatch = node;

        if (_currentSearchMatch is not null)
        {
            _currentSearchMatch.IsCurrentSearchMatch = true;
            _currentSearchMatch.UpdateSearchHighlight(
                query,
                highlightBackground,
                highlightForeground,
                normalForeground,
                currentBackground);
        }
    }

    private void CollapseAllExceptRoot(TreeNodeViewModel node)
    {
        foreach (var child in node.Children)
        {
            child.IsExpanded = false;
            CollapseAllExceptRoot(child);
        }
    }

    private (IBrush highlightBackground, IBrush highlightForeground, IBrush normalForeground, IBrush currentBackground)
        GetSearchHighlightBrushes()
    {
        var app = global::Avalonia.Application.Current;
        var theme = app?.ActualThemeVariant ?? ThemeVariant.Light;

        IBrush highlightBackground = new SolidColorBrush(Color.Parse("#FFEB3B"));
        IBrush highlightForeground = new SolidColorBrush(Color.Parse("#000000"));
        IBrush normalForeground = theme == ThemeVariant.Dark
            ? new SolidColorBrush(Color.Parse("#E7E9EF"))
            : new SolidColorBrush(Color.Parse("#1A1A1A"));
        IBrush currentBackground = new SolidColorBrush(Color.Parse("#F9A825"));

        if (app?.Resources.TryGetResource("TreeSearchHighlightBrush", theme, out var bg) == true &&
            bg is IBrush bgBrush)
            highlightBackground = bgBrush;

        if (app?.Resources.TryGetResource("TreeSearchHighlightTextBrush", theme, out var fg) == true &&
            fg is IBrush fgBrush)
            highlightForeground = fgBrush;

        if (app?.Resources.TryGetResource("TreeSearchCurrentBrush", theme, out var current) == true &&
            current is IBrush currentBrush)
            currentBackground = currentBrush;

        if (app?.Resources.TryGetResource("AppTextBrush", theme, out var textFg) == true && textFg is IBrush textBrush)
            normalForeground = textBrush;

        return (highlightBackground, highlightForeground, normalForeground, currentBackground);
    }
}
