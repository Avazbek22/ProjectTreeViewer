using Avalonia.Controls.Documents;
using Avalonia.Media;
using ProjectTreeViewer.Kernel.Contracts;

namespace ProjectTreeViewer.Avalonia.ViewModels;

public sealed class TreeNodeViewModel : ViewModelBase
{
    private bool _isChecked;
    private bool _isExpanded;
    private bool _isSelected;
    private string _displayName;

    public TreeNodeViewModel(
        TreeNodeDescriptor descriptor,
        TreeNodeViewModel? parent,
        IImage? icon)
    {
        Descriptor = descriptor;
        Parent = parent;
        _displayName = descriptor.DisplayName;
        Icon = icon;
    }

    public TreeNodeDescriptor Descriptor { get; }

    public TreeNodeViewModel? Parent { get; }

    public IList<TreeNodeViewModel> Children { get; } = new List<TreeNodeViewModel>();

    public IImage? Icon { get; set; }

    public InlineCollection DisplayInlines { get; } = new();

    public string DisplayName
    {
        get => _displayName;
        set
        {
            if (_displayName == value) return;
            _displayName = value;
            RaisePropertyChanged();
        }
    }

    public string FullPath => Descriptor.FullPath;

    public bool IsChecked
    {
        get => _isChecked;
        set
        {
            if (_isChecked == value) return;
            SetChecked(value, updateChildren: true, updateParent: true);
        }
    }

    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (_isExpanded == value) return;
            _isExpanded = value;
            RaisePropertyChanged();
        }
    }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;
            _isSelected = value;
            RaisePropertyChanged();
        }
    }

    public void SetExpandedRecursive(bool expanded)
    {
        IsExpanded = expanded;
        foreach (var child in Children)
            child.SetExpandedRecursive(expanded);
    }

    public IEnumerable<TreeNodeViewModel> Flatten()
    {
        yield return this;
        foreach (var child in Children)
        {
            foreach (var descendant in child.Flatten())
                yield return descendant;
        }
    }

    public void EnsureParentsExpanded()
    {
        var current = Parent;
        while (current is not null)
        {
            current.IsExpanded = true;
            current = current.Parent;
        }
    }

    public void UpdateIcon(IImage? icon)
    {
        Icon = icon;
        RaisePropertyChanged(nameof(Icon));
    }

    public void UpdateSearchHighlight(string? query, IBrush? background, IBrush? foreground)
    {
        DisplayInlines.Clear();

        if (string.IsNullOrWhiteSpace(query))
        {
            DisplayInlines.Add(new Run(DisplayName));
            RaisePropertyChanged(nameof(DisplayInlines));
            return;
        }

        var startIndex = 0;
        while (startIndex < DisplayName.Length)
        {
            var index = DisplayName.IndexOf(query, startIndex, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
            {
                DisplayInlines.Add(new Run(DisplayName[startIndex..]));
                break;
            }

            if (index > startIndex)
                DisplayInlines.Add(new Run(DisplayName[startIndex..index]));

            DisplayInlines.Add(new Run(DisplayName.Substring(index, query.Length))
            {
                Background = background,
                Foreground = foreground
            });

            startIndex = index + query.Length;
        }

        if (DisplayInlines.Count == 0)
            DisplayInlines.Add(new Run(DisplayName));

        RaisePropertyChanged(nameof(DisplayInlines));
    }

    private void SetChecked(bool value, bool updateChildren, bool updateParent)
    {
        _isChecked = value;
        RaisePropertyChanged(nameof(IsChecked));

        if (updateChildren)
        {
            foreach (var child in Children)
                child.SetChecked(value, updateChildren: true, updateParent: false);
        }

        if (updateParent)
            Parent?.UpdateCheckedFromChildren();
    }

    private void UpdateCheckedFromChildren()
    {
        bool allChecked = Children.Count > 0 && Children.All(child => child.IsChecked);
        if (_isChecked != allChecked)
        {
            _isChecked = allChecked;
            RaisePropertyChanged(nameof(IsChecked));
        }

        Parent?.UpdateCheckedFromChildren();
    }
}
