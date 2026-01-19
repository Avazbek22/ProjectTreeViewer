using ProjectTreeViewer.Infrastructure.Elevation;
using Xunit;

namespace ProjectTreeViewer.Tests.Integration;

public sealed class ElevationServiceTests
{
	[Fact]
	public void IsAdministrator_ReturnsFalseOnNonWindows()
	{
		var service = new ElevationService();

		if (!OperatingSystem.IsWindows())
			Assert.False(service.IsAdministrator);
	}

	[Fact]
	public void TryRelaunchAsAdministrator_ReturnsFalseOnNonWindows()
	{
		var service = new ElevationService();

		if (!OperatingSystem.IsWindows())
			Assert.False(service.TryRelaunchAsAdministrator(Kernel.Models.CommandLineOptions.Empty));
	}
}
