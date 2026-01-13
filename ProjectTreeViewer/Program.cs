namespace ProjectTreeViewer
{
	internal static class Program
	{
		[STAThread]
		private static void Main(string[] args)
		{
			ApplicationConfiguration.Initialize();

			var options = CommandLineOptions.Parse(args);
			Application.Run(new Form1(options));
		}
	}
}
