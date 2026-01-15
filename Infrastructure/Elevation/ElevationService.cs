using System.ComponentModel;
using System.Diagnostics;
using System.Security.Principal;
using ProjectTreeViewer.Kernel.Abstractions;
using ProjectTreeViewer.Kernel.Models;

namespace ProjectTreeViewer.Infrastructure.Elevation;

public sealed class ElevationService : IElevationService
{
	public bool IsAdministrator
	{
		get
		{
			if (!OperatingSystem.IsWindows()) return false;

			using var identity = WindowsIdentity.GetCurrent();
			var principal = new WindowsPrincipal(identity);
			return principal.IsInRole(WindowsBuiltInRole.Administrator);
		}
	}

	public bool TryRelaunchAsAdministrator(CommandLineOptions options)
	{
		if (!OperatingSystem.IsWindows()) return false;

		try
		{
			var exePath = Environment.ProcessPath;
			if (string.IsNullOrWhiteSpace(exePath)) return false;

			var psi = new ProcessStartInfo
			{
				FileName = exePath,
				UseShellExecute = true,
				Verb = "runas",
				Arguments = options.ToArguments()
			};

			Process.Start(psi);
			return true;
		}
		catch (Win32Exception ex) when (ex.NativeErrorCode == 1223)
		{
			return false;
		}
	}
}
