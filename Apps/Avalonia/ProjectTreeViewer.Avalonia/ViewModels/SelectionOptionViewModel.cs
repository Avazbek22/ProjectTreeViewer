namespace ProjectTreeViewer.Avalonia.ViewModels;

public sealed class SelectionOptionViewModel : ViewModelBase
{
    private bool _isChecked;

    public SelectionOptionViewModel(string name, bool isChecked)
    {
        Name = name;
        _isChecked = isChecked;
    }

    public string Name { get; }

    public bool IsChecked
    {
        get => _isChecked;
        set
        {
            if (_isChecked == value) return;
            _isChecked = value;
            RaisePropertyChanged();
            CheckedChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? CheckedChanged;
}
