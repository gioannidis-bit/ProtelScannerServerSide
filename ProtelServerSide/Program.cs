using System;
using System.Windows.Forms;
using ProtelScannerServerSide;

namespace ProtelServerSide;

internal static class Program
{
	[STAThread]
	private static void Main(string[] args)
	{
		long leistacc = 0L;
		if (args.Length != 0)
		{
			long.TryParse(args[0], out leistacc);
		}
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(defaultValue: false);
		if (leistacc > 0)
		{
			Application.Run(new fInformProfiles(args));
		}
		else
		{
			Application.Run(new fConfig());
		}
	}
}
