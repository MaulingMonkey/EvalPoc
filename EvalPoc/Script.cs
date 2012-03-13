using System.Drawing;
using System.Windows.Forms;

namespace EvalPoc
{
	public interface IScript
	{
		void Run( PaintEventArgs args );
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

		public void Run( PaintEventArgs args )
		{
			var fx = args.Graphics;
			fx.Clear( SystemColors.Control );
			fx.DrawString( Message, Font, Brushes.Black, 20, 20 );
		}
	}
}
