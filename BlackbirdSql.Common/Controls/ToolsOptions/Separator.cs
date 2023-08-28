#region Assembly Microsoft.SqlServer.Management.Controls, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLCommon\Microsoft.SqlServer.Management.Controls.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;




// namespace Microsoft.SqlServer.Management.Controls
namespace BlackbirdSql.Common.Controls.ToolsOptions
{
	public sealed class Separator : Label
	{
		private const int C_Spacing = 2;

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new Image Image => null;

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new ImageList ImageList => null;

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new ContentAlignment ImageAlign => ContentAlignment.TopLeft;

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new int ImageIndex => -1;

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			Size size = TextRenderer.MeasureText(Text, Font);
			if (size.Width + C_Spacing > Width)
			{
				return;
			}

			int x = 0;
			int x2 = 0;
			int x3 = 0;
			int x4 = 0;
			int num = -1;
			int num2 = 0;
			if (TextAlign == ContentAlignment.TopLeft || TextAlign == ContentAlignment.MiddleLeft || TextAlign == ContentAlignment.BottomLeft)
			{
				x = size.Width + C_Spacing;
				x2 = Width;
				num = 0;
			}
			else if (TextAlign == ContentAlignment.BottomCenter || TextAlign == ContentAlignment.MiddleCenter || TextAlign == ContentAlignment.TopCenter)
			{
				x = 0;
				x2 = Width / C_Spacing - size.Width / C_Spacing - C_Spacing;
				x3 = Width / C_Spacing + size.Width / C_Spacing + C_Spacing;
				x4 = Width;
				num = 1;
			}
			else if (TextAlign == ContentAlignment.BottomRight || TextAlign == ContentAlignment.MiddleRight || TextAlign == ContentAlignment.TopRight)
			{
				x = 0;
				x2 = Width - size.Width - C_Spacing;
				num = C_Spacing;
			}

			if (TextAlign == ContentAlignment.TopLeft || TextAlign == ContentAlignment.TopCenter || TextAlign == ContentAlignment.TopRight)
			{
				num2 = size.Height / C_Spacing;
			}
			else if (TextAlign == ContentAlignment.MiddleCenter || TextAlign == ContentAlignment.MiddleLeft || TextAlign == ContentAlignment.MiddleRight)
			{
				num2 = Height / C_Spacing;
			}
			else if (TextAlign == ContentAlignment.BottomCenter || TextAlign == ContentAlignment.BottomRight || TextAlign == ContentAlignment.BottomLeft)
			{
				num2 = Height - size.Height / C_Spacing;
			}

			if (RightToLeft == RightToLeft.Yes)
			{
				switch (num)
				{
					case 0:
						x = 0;
						x2 = Width - size.Width - C_Spacing;
						break;
					default:
						x = size.Width + C_Spacing;
						x2 = Width;
						break;
					case 1:
						break;
				}
			}

			e.Graphics.DrawLine(SystemPens.ControlDark, x, num2, x2, num2);
			if (num == 1)
			{
				e.Graphics.DrawLine(SystemPens.ControlDark, x3, num2, x4, num2);
			}

			num2++;
			e.Graphics.DrawLine(SystemPens.ControlLightLight, x, num2, x2, num2);
			if (num == 1)
			{
				e.Graphics.DrawLine(SystemPens.ControlLightLight, x3, num2, x4, num2);
			}
		}
	}
}
