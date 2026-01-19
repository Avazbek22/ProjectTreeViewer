using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ProjectTreeViewer.Avalonia.Services;
using ProjectTreeViewer.Kernel.Models;

namespace ProjectTreeViewer.Avalonia;

public sealed class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var options = CommandLineOptions.Parse(desktop.Args ?? Array.Empty<string>());
            var services = AvaloniaCompositionRoot.CreateDefault(options);
            desktop.MainWindow = new MainWindow(options, services);
        }

        base.OnFrameworkInitializationCompleted();
    }
}
