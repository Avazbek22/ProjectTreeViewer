using System.ComponentModel;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows.Forms;

namespace ProjectTreeViewer;

public sealed class ElevationService
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
			var psi = new ProcessStartInfo
			{
				FileName = Application.ExecutablePath,
				UseShellExecute = true,
				Verb = "runas",
				Arguments = options.ToArguments()
			};

			Process.Start(psi);
			return true;
		}
		catch (Win32Exception ex) when (ex.NativeErrorCode == 1223)
		{
			// Пользователь отменил UAC
			return false;
		}
	}
}
