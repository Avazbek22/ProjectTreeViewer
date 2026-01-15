using ProjectTreeViewer.Kernel.Models;
using ProjectTreeViewer.WinForms.Services;

namespace ProjectTreeViewer.WinForms;

internal static class Program
{
	[STAThread]
	private static void Main(string[] args)
	{
		ApplicationConfiguration.Initialize();

		var options = CommandLineOptions.Parse(args);
		var services = WinFormsCompositionRoot.CreateDefault(options);
		Application.Run(new Form1(options, services));
	}
}
