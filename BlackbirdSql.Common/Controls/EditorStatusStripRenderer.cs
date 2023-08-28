// Microsoft.VisualStudio.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.Data.Tools.Design.Core.Controls.EditorStatusStripRenderer

using System.Windows.Forms;


namespace BlackbirdSql.Common.Controls
{
	public class EditorStatusStripRenderer : ToolStripProfessionalRenderer
	{
		private readonly StatusStrip statusStrip;

		public EditorStatusStripRenderer(StatusStrip statusStrip)
		{
			this.statusStrip = statusStrip;
		}

		protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
		{
			if (e.Item is ToolStripStatusLabel && SystemInformation.HighContrast)
			{
				e.TextColor = statusStrip.ForeColor;
			}

			base.OnRenderItemText(e);
		}
	}
}
