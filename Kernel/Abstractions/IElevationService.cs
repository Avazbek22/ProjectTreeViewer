using ProjectTreeViewer.Kernel.Models;

namespace ProjectTreeViewer.Kernel.Abstractions;

public interface IElevationService
{
	bool IsAdministrator { get; }
	bool TryRelaunchAsAdministrator(CommandLineOptions options);
}
