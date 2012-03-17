using System;
using System.Windows.Forms;

namespace EvalPoc
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			SlimDX.Vector2 v2; // force SlimDX to be referenced

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new EvalPocForm());
		}
	}
}
