using System;
using Avalonia.Threading;

namespace ProjectTreeViewer.Avalonia.Coordinators;

public sealed class NameFilterCoordinator : IDisposable
{
    private readonly Action _applyFilterRealtime;
    private readonly System.Timers.Timer _filterDebounceTimer;

    public NameFilterCoordinator(Action applyFilterRealtime)
    {
        _applyFilterRealtime = applyFilterRealtime;
        _filterDebounceTimer = new System.Timers.Timer(300)
        {
            AutoReset = false
        };
        _filterDebounceTimer.Elapsed += (_, _) =>
        {
            Dispatcher.UIThread.Post(_applyFilterRealtime);
        };
    }

    public void OnNameFilterChanged()
    {
        _filterDebounceTimer.Stop();
        _filterDebounceTimer.Start();
    }

    public void Dispose()
    {
        _filterDebounceTimer.Stop();
        _filterDebounceTimer.Dispose();
    }
}
