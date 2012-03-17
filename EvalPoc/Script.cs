using System.Drawing;
using System.Windows.Forms;

namespace EvalPoc
{
	public class ScriptRunArgs : PaintEventArgs
	{
		public readonly float T;
		public readonly float DT;

		public ScriptRunArgs( Graphics fx, Rectangle area, float t, float dt )
			: base( fx, area )
		{
			T = t;
			DT = dt;
		}
	}

	public interface IScript
	{
		void Run( ScriptRunArgs args );
	}

	class ErrorScript : IScript
	{
		readonly string Message;
		readonly Font   Font;

		public ErrorScript( Font font, string message )
		{
			Font    = font;
			Message = message;
		}

		public void Run( ScriptRunArgs args )
		{
			var fx = args.Graphics;
			fx.Clear( SystemColors.Control );
			fx.DrawString( Message, Font, Brushes.Black, 20, 20 );
		}
	}
}
